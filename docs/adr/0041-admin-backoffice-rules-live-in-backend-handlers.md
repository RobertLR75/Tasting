# Admin backoffice rules live in backend handlers

Admin-frontendet skal være en tynn klient over API-et, og alle forretningsregler for adminflytene skal håndheves i backendens `IRequestHandler`-lag, ikke i Blazor-komponenter eller FastEndpoints-klasser. Vi velger dette fordi dagens arkitektur allerede skiller transport fra domene, og fordi regler som rollekrav, statusgating, unikhet og siste aktive admin må være kanoniske uansett klient.
