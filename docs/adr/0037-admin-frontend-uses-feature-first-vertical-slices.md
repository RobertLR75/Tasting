# Admin frontend uses feature-first vertical slices

Admin Frontend organiseres som feature-first, domeneorienterte slices i stedet for generiske mapper for pages, services og models. Vi velger dette fordi adminflaten allerede er delt i tydelige backoffice-kapasiteter, og slices gir sterkere modulgrenser for `Arrangement`, `Identity`, `Catalog` og `Results` enn en lagdelt struktur som lett blander regler, API-kall og komponenter på tvers av domener.
