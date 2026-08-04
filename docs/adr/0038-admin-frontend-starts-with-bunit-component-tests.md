# Admin frontend starts with bUnit component tests

Admin Frontend starter med komponenttester basert på bUnit og xUnit som primær teststrategi, før vi innfører tyngre ende-til-ende-tester. Vi velger dette fordi adminappen er en Blazor/MudBlazor-backoffice med tydelige feature-slices, og komponenttester gir rask verifisering av rendering, routing og navigasjon uten å låse oss tidlig til browserautomatisering mens sidene fortsatt er blanke.
