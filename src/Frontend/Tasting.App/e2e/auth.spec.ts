import { expect, test } from '@playwright/test';

test('an unauthenticated participant is redirected to login', async ({ page }) => {
  await page.goto('/arrangements');

  await expect(page).toHaveURL(/\/login$/);
  await expect(page.getByRole('heading', { name: 'Tasting' })).toBeVisible();
});

test('a participant can log in and the session survives a reload', async ({ page }) => {
  await page.route('**/api/v1/participant/arrangements', route => route.fulfill({ status: 200, contentType: 'application/json', body: '{"items":[]}' }));
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
  await expect(page.getByRole('heading', { name: 'Aktive arrangementer' })).toBeVisible();

  await page.reload();
  await expect(page).toHaveURL(/\/arrangements$/);
  await expect(page.getByText('Hei, Pat')).toBeVisible();
});

test('a participant can browse active arrangements and self-join', async ({ page }) => {
  await page.addInitScript(token => localStorage.setItem('tasting.participant.session', JSON.stringify({
    token, email: 'participant@tasting.no', firstName: 'Pat', lastName: 'Ticipant', role: 'User',
  })), createToken(Date.now() + 60_000));
  await page.route('**/api/v1/participant/arrangements', route => route.fulfill({
    status: 200, contentType: 'application/json',
    body: JSON.stringify({ items: [{ id: 'arr-1', name: 'Sommerfest', description: 'Blindsmaking', joined: false }] }),
  }));
  await page.route('**/api/v1/participant/arrangements/arr-1/join', route => route.fulfill({
    status: 200, contentType: 'application/json', body: JSON.stringify({ id: 'arr-1', name: 'Sommerfest', status: 'Active' }),
  }));
  await page.route('**/api/v1/participant/arrangements/arr-1', route => route.fulfill({
    status: 200, contentType: 'application/json',
    body: JSON.stringify({ id: 'arr-1', name: 'Sommerfest', status: 'Active', beers: [] }),
  }));

  await page.goto('/arrangements');
  await expect(page.getByRole('heading', { name: 'Sommerfest' })).toBeVisible();
  await page.getByRole('button', { name: 'Bli med' }).click();

  await expect(page).toHaveURL(/\/arrangements\/arr-1\/lobby$/);
  await expect(page.getByRole('heading', { name: 'Du er med' })).toBeVisible();
});

test('a rejected self-join shows the backend error and stays on discovery', async ({ page }) => {
  await page.addInitScript(token => localStorage.setItem('tasting.participant.session', JSON.stringify({
    token, email: 'participant@tasting.no', firstName: 'Pat', lastName: 'Ticipant', role: 'User',
  })), createToken(Date.now() + 60_000));
  await page.route('**/api/v1/participant/arrangements', route => route.fulfill({
    status: 200, contentType: 'application/json',
    body: JSON.stringify({ items: [{ id: 'arr-1', name: 'Sommerfest', joined: false }] }),
  }));
  await page.route('**/api/v1/participant/arrangements/arr-1/join', route => route.fulfill({
    status: 409, contentType: 'application/json',
    body: JSON.stringify({ code: 'conflict', message: 'Arrangementet kan ikke lenger ta imot deltakere.', correlationId: 'corr-join' }),
  }));

  await page.goto('/arrangements');
  await page.getByRole('button', { name: 'Bli med' }).click();

  await expect(page.getByRole('alert')).toHaveText('Arrangementet kan ikke lenger ta imot deltakere.');
  await expect(page).toHaveURL(/\/arrangements$/);
});

test('a joined participant waits without seeing beer details before Started', async ({ page }) => {
  await page.addInitScript(token => localStorage.setItem('tasting.participant.session', JSON.stringify({
    token, email: 'participant@tasting.no', firstName: 'Pat', lastName: 'Ticipant', role: 'User',
  })), createToken(Date.now() + 60_000));
  await page.route('**/api/v1/participant/arrangements/arr-1', route => route.fulfill({
    status: 200, contentType: 'application/json',
    body: JSON.stringify({ id: 'arr-1', name: 'Sommerfest', status: 'Active', beers: [] }),
  }));

  await page.goto('/arrangements/arr-1/lobby');

  await expect(page.getByRole('heading', { name: 'Sommerfest' })).toBeVisible();
  await expect(page.getByText('Venter på at arrangementet starter…')).toBeVisible();
  await expect(page.getByText('Secret beer')).toHaveCount(0);
});

test('participant arrangement backend errors are shown cleanly', async ({ page }) => {
  await page.addInitScript(token => localStorage.setItem('tasting.participant.session', JSON.stringify({
    token, email: 'participant@tasting.no', firstName: 'Pat', lastName: 'Ticipant', role: 'User',
  })), createToken(Date.now() + 60_000));
  await page.route('**/api/v1/participant/arrangements/missing', route => route.fulfill({
    status: 404, contentType: 'application/json',
    body: JSON.stringify({ code: 'not_found', message: 'Arrangementet ble ikke funnet.', correlationId: 'corr-state' }),
  }));

  await page.goto('/arrangements/missing/lobby');

  await expect(page.getByRole('alert')).toHaveText('Arrangementet ble ikke funnet.');
});

test('a joined participant can view backend-ranked results after completion', async ({ page }) => {
  await page.addInitScript(token => localStorage.setItem('tasting.participant.session', JSON.stringify({
    token, email: 'participant@tasting.no', firstName: 'Pat', lastName: 'Ticipant', role: 'User',
  })), createToken(Date.now() + 60_000));
  await page.route('**/api/v1/participant/arrangements/arr-1', route => route.fulfill({
    status: 200, contentType: 'application/json',
    body: JSON.stringify({ id: 'arr-1', name: 'Sommerfest', status: 'Completed', beers: [
      { id: 'beer-2', name: 'Backend Winner', breweryName: 'Vinnerbryggeriet', beerStyle: 'IPA', beerType: 'Ale' },
      { id: 'beer-1', name: 'Backend Runner-up', breweryName: 'Andreplassen', beerStyle: 'Lager', beerType: 'Lager' },
    ] }),
  }));
  await page.route('**/api/v1/arrangements/arr-1/results', route => route.fulfill({
    status: 200, contentType: 'application/json',
    body: JSON.stringify({ results: [
      { rank: 1, beerId: 'beer-2', beerNameSnapshot: 'Backend Winner', totalRating: 8.2, ratingCount: 2, standardDeviation: 0.4, participants: [] },
      { rank: 2, beerId: 'beer-1', beerNameSnapshot: 'Backend Runner-up', totalRating: 8.2, ratingCount: 3, standardDeviation: 0.1, participants: [] },
    ] }),
  }));

  await page.goto('/arrangements/arr-1/lobby');

  await expect(page).toHaveURL(/\/arrangements\/arr-1\/results$/);
  await expect(page.getByRole('heading', { name: 'Sommerfest — Resultater' })).toBeVisible();
  await expect(page.getByRole('row').nth(1)).toContainText('1Backend WinnerVinnerbryggeriet8.20');
  await expect(page.getByRole('row').nth(2)).toContainText('2Backend Runner-upAndreplassen8.20');
});

test('completed results show an explicit empty state', async ({ page }) => {
  await page.addInitScript(token => localStorage.setItem('tasting.participant.session', JSON.stringify({
    token, email: 'participant@tasting.no', firstName: 'Pat', lastName: 'Ticipant', role: 'User',
  })), createToken(Date.now() + 60_000));
  await page.route('**/api/v1/participant/arrangements/arr-1', route => route.fulfill({
    status: 200, contentType: 'application/json',
    body: JSON.stringify({ id: 'arr-1', name: 'Sommerfest', status: 'Completed', beers: [] }),
  }));
  await page.route('**/api/v1/arrangements/arr-1/results', route => route.fulfill({
    status: 200, contentType: 'application/json', body: '{"results":[]}',
  }));

  await page.goto('/arrangements/arr-1/results');
  await expect(page.getByText('Ingen resultater er tilgjengelige.')).toBeVisible();
});

test('completed results show the backend error surface', async ({ page }) => {
  await page.addInitScript(token => localStorage.setItem('tasting.participant.session', JSON.stringify({
    token, email: 'participant@tasting.no', firstName: 'Pat', lastName: 'Ticipant', role: 'User',
  })), createToken(Date.now() + 60_000));
  await page.route('**/api/v1/participant/arrangements/arr-1', route => route.fulfill({
    status: 200, contentType: 'application/json',
    body: JSON.stringify({ id: 'arr-1', name: 'Sommerfest', status: 'Completed', beers: [] }),
  }));
  await page.route('**/api/v1/arrangements/arr-1/results', route => route.fulfill({
    status: 403, contentType: 'application/json', body: JSON.stringify({
      code: 'forbidden', message: 'Du er ikke deltaker i arrangementet.', correlationId: 'corr-results',
    }),
  }));

  await page.goto('/arrangements/arr-1/results');
  await expect(page.getByRole('alert')).toHaveText('Du er ikke deltaker i arrangementet.');
});

test('an invalid arrangement status response replaces the lobby content', async ({ page }) => {
  await page.addInitScript(token => localStorage.setItem('tasting.participant.session', JSON.stringify({
    token, email: 'participant@tasting.no', firstName: 'Pat', lastName: 'Ticipant', role: 'User',
  })), createToken(Date.now() + 60_000));
  await page.route('**/api/v1/participant/arrangements/canceled', route => route.fulfill({
    status: 409, contentType: 'application/json',
    body: JSON.stringify({ code: 'conflict', message: 'Arrangementet er avlyst.', correlationId: 'corr-state' }),
  }));

  await page.goto('/arrangements/canceled/lobby');

  await expect(page.getByRole('alert')).toHaveText('Arrangementet er avlyst.');
  await expect(page.getByText('Venter på at arrangementet starter…')).toHaveCount(0);
});

test('an unauthorized arrangement response returns the participant to login', async ({ page }) => {
  await page.addInitScript(token => localStorage.setItem('tasting.participant.session', JSON.stringify({
    token, email: 'participant@tasting.no', firstName: 'Pat', lastName: 'Ticipant', role: 'User',
  })), createToken(Date.now() + 60_000));
  await page.route('**/api/v1/participant/arrangements/expired', route => route.fulfill({
    status: 401, contentType: 'application/json',
    body: JSON.stringify({ code: 'unauthorized', message: 'Authentication is required.', correlationId: 'corr-auth' }),
  }));

  await page.goto('/arrangements/expired/lobby');

  await expect(page).toHaveURL(/\/login$/);
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
