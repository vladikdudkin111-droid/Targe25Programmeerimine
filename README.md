# TARge25Shop

V14 on õpetaja arhiivi ja meie V13 võrdluse põhjal koostatud ühine lõppkoopia.
Uuesti saadetud õpetaja ZIP on bait-baidilt sama varasema õpetaja arhiiviga.
Seetõttu säilitab V14 V13 tööloogika: uut õpetaja funktsionaalsust selles ZIP-is ei olnud.
Failis MERGE_REPORT.md on venekeelne võrdlustabel ja ühendamise põhjendused.

Projekt järgib õpetaja neljakihilist ülesehitust ja Create/Update töövoogu.
Kõik selgitavad kommentaarid koodis on eesti keeles.

V13 lisab ühe pildi kustutamise Update ja Details lehel. Nupp kasutab õpetaja
nimega `RemoveImageFromApi` teenusemeetodit. Pilt kustutatakse POST päringuga
koos antiforgery tokeniga, ülejäänud kosmoselaeva andmeid muutmata.

Andmebaasimudel ja migratsioonid on V12-ga samad. Uuendus ei paranda automaatselt
varasemat `FileToApis already exists` migratsioonikonflikti.

Testid: `node --test tests/single-image-delete.test.cjs`.
Need kontrollivad JavaScripti ja lähtekoodi lepinguid, mitte .NET/SQL käivitamist.

Käivitamise sammud on failis `START_HERE.txt`.
Varasema andmebaasivea uurimiseks on kaasas ainult lugemist tegev `CHECK_DATABASE.sql`.
