import { expect, test } from '@playwright/test';

test('an unauthenticated participant is redirected to login', async ({ page }) => {
  await page.goto('/arrangements');

  await expect(page).toHaveURL(/\/login$/);
  await expect(page.getByRole('heading', { name: 'Tasting' })).toBeVisible();
});

test('a participant can log in and the session survives a reload', async ({ page }) => {
  await page.route('**/api/v1/users/login', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        token: createToken(Date.now() + 60_000),
        email: 'participant@tasting.no',
        firstName: 'Pat',
        lastName: 'Ticipant',
        role: 'User',
      }),
    });
  });
  await page.goto('/login');
  await page.getByLabel('E-post').fill('participant@tasting.no');
  await page.getByLabel('Passord').fill('password123');
  await page.getByRole('button', { name: 'Logg inn' }).click();

  await expect(page).toHaveURL(/\/arrangements$/);
  await expect(page.getByRole('heading', { name: 'Mine arrangementer' })).toBeVisible();

  await page.reload();
  await expect(page).toHaveURL(/\/arrangements$/);
  await expect(page.getByText('Hei, Pat')).toBeVisible();
});

function createToken(expiresAt: number): string {
  return `header.${Buffer.from(JSON.stringify({ exp: Math.floor(expiresAt / 1000) })).toString('base64url')}.signature`;
}

test('backend authentication failures are shown without leaking details', async ({ page }) => {
  await page.route('**/api/v1/users/login', async (route) => {
    await route.fulfill({
      status: 401,
      contentType: 'application/json',
      body: JSON.stringify({
        code: 'unauthorized',
        message: 'Invalid email or password.',
        correlationId: 'test-correlation',
      }),
    });
  });
  await page.goto('/login');
  await page.getByLabel('E-post').fill('participant@tasting.no');
  await page.getByLabel('Passord').fill('wrong-password');
  await page.getByRole('button', { name: 'Logg inn' }).click();

  await expect(page.getByRole('alert')).toHaveText('Ugyldig e-post eller passord.');
  await expect(page).toHaveURL(/\/login$/);
});

test('non-credential API failures use the backend error message', async ({ page }) => {
  await page.route('**/api/v1/users/login', async (route) => {
    await route.fulfill({
      status: 503,
      contentType: 'application/json',
      body: JSON.stringify({
        code: 'service_unavailable',
        message: 'Tjenesten er midlertidig utilgjengelig.',
        correlationId: 'test-correlation',
      }),
    });
  });
  await page.goto('/login');
  await page.getByLabel('E-post').fill('participant@tasting.no');
  await page.getByLabel('Passord').fill('password123');
  await page.getByRole('button', { name: 'Logg inn' }).click();

  await expect(page.getByRole('alert')).toHaveText('Tjenesten er midlertidig utilgjengelig.');
});
