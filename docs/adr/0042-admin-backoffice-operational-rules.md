# Admin backoffice operational rules

Admin-backoffice bruker full-lister som standard for arrangementer og users, men arbeidsflytene er fortsatt strengt gated av domeneregler: bare `Admin` kan logge inn, arrangementmutasjoner er bare lov i `Created`, kun aktive users kan legges til som participants, og bare aktive breweries/beers kan legges til i et arrangement. Vi velger dette fordi backoffice trenger komplett operativ oversikt samtidig som medlemskap, katalogvalg og brukeradministrasjon må være konsistente med domenets livssyklus- og identitetsregler.
