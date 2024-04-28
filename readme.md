# Půjčovadlo (backend)
Autor: Bc. Jaroslav Fikar (fikarja3@fit.cvut.cz)

Půjčovadlo je mobilní aplikace, která umožňuje peer-to-peer sdílení věcí mezi uživateli.

## Informace o aplikaci
Celý systém se skládá z backendové části a mobilních klientských aplikací.

Backendová aplikace je implementována v jazyce C# a využívá známé frameworky jako je .NET Core, ASP.NET Core nebo Entity Framework Core.

## Běhové prostředí
Aplikace se zamýšlena pro nasazení na VPS, ale návrh aplikace umožňuje i nasazení na cloudu nebo v rámci kontejnerizace.

## Požadavky
- .NET Core 8 (instalace viz [dokumentace](https://learn.microsoft.com/en-us/dotnet/core/install))
- databáze PostgreSQL
- běžící služba MinIO

## Spuštění a instalace
1. Stažení zdrojových kódů aplikace z tohoto repozitáře.
1. Instalace databáze a objektového úložiště. Tyto služby lze snadno připravit např. pomocí nástroje docker-compose. 
Připravený docker-compose.yaml není zamýšlen k použití na produkčním prostředí a jeho hlavním účelem je usnadnit přípravu vývojářského prostředí.

   Spuštění docker-compose: 
    ```bash
    docker-compose up
    ```
      
1. Vytvoření a nastavení konfiguračního souboru `PujcovadloServer/appsettings.json`.
   1. Lze využít ukázkový konfigurační soubor `PujcovadloServer/appsettings.example.json`.
   1. Konfigurace databáze pomocí `ConnectionStrings:DefaultConnection`. Ukázková konfigurační soubor obsahuje požadovaný formát.
   Více k pravidlům pro konfiguraci databáze viz [dokumentace](https://learn.microsoft.com/en-us/ef/core/miscellaneous/connection-strings).
   1. Konfigurace `JWT.Secret` pro vydávání JWT tokenů.
   1. Konfigurace `MinIO` pro připojení k MinIO instanci.
   1. Konfigurace `Syncfusion` a zadání licenčního klíče. 

1. Instalace databáze: `dotnet ef database update`
1. Spuštění aplikace: `dotnet run`

## Ngrok
Pro vzdálený přístup k aplikaci lze využít nástroj Ngrok, který umožňuje vytvořit veřejnou URL adresu pro přístup k aplikaci. 
To může usnadnit testování klientských aplikací na skutečných i simulovaných zařízeních.
K dispozici je konfigurační soubor `ngrok.example.yml`. Použití služby vyžaduje registraci na stránkách [Ngrok](https://ngrok.com/).
Pro správnou funkčnost klientských aplikací je třeba, aby byla veřejně dostupná i služba MinIO, protože ji využívají pro získávání souborů.

Spuštění služby NGrok s konfiguračním souborem:
```bash
ngrok start --config ngrok.example.yaml --all
```

## Spuštění testů
Spuštění testů je možné jednotlivě v rámci jednotlivých testových projektů.

### Jednotkové testy
Spuštění jednotkových testů:
```bash
dotnet test Tests
```

### Funkční testy
Spuštění funkčních testů vyžaduje spuštěnou instanci databáze. 
Úložiště MinIO není vyžadováno, protože funkční testy využívají simulové úložiště.
Testovací databáze je připravena v `docker-compose.yaml` jako služba `db_test` nebo ji lze vytvořit příkazem v souboru `create_test_db.sh`.

Konfigurace testovacího prostředí je možná v souboru `PujcovadloServer/testappsettings.json`. 
Testovací konfigurace je možné vytvořit zkopírováním `PujcovadloServer/appsettings.json`, kde stačí změnit připojení k databázi tak, aby byla použita testovací databáze.

Spuštění funkčních testů:
```bash
dotnet test FunctionalTests
```
