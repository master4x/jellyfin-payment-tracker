# Jellyfin Payment Tracker

Internes Jellyfin-Server-Plugin (Jellyfin 12.2 / .NET 10) zur monatsweisen Verwaltung von Barzahlungen.
Alle Daten liegen in der Plugin-Konfiguration (`<data>/plugins/configurations/Jellyfin.Plugin.PaymentTracker.xml`) – keine externe Datenbank.

## Status-Logik

| Status    | Bedingung                              | Farbe         |
|-----------|----------------------------------------|---------------|
| `GRACE`   | `IsInGracePeriod == true` (hat Vorrang)| 🎁 Lila       |
| `PAID`    | bezahlt bis > aktueller Monat          | 🟢 Grün       |
| `DUE`     | bezahlt bis == aktueller Monat         | 🟩 Gelbgrün   |
| `OVERDUE` | bezahlt bis < aktueller Monat          | 🔴 Rot        |

## API (nur Administratoren)

| Methode | Route                        | Body                                   |
|---------|------------------------------|----------------------------------------|
| GET     | `/PaymentTracker/Users`      | –                                      |
| POST    | `/PaymentTracker/AddMonth`   | `{ "UserId": "..." }`                  |
| POST    | `/PaymentTracker/SetMonth`   | `{ "UserId": "...", "Year": 2026, "Month": 11 }` |
| POST    | `/PaymentTracker/ToggleGrace`| `{ "UserId": "..." }`                  |
| POST    | `/PaymentTracker/UpdateNotes`| `{ "UserId": "...", "Notes": "..." }`  |

## Bauen & Veröffentlichen

```powershell
./build.ps1 -Version 1.0.0.0
```

1. ZIP aus `artifacts/` als Asset eines GitHub-Releases `v1.0.0.0` hochladen.
2. Aktualisierte `manifest.json` committen und pushen.
3. In Jellyfin unter *Dashboard → Plugins → Repositories* die Raw-URL eintragen:
   `https://raw.githubusercontent.com/master4x/jellyfin-payment-tracker/master/manifest.json`

> **Hinweis:** Solange das Repository privat ist, kann Jellyfin weder `manifest.json` noch das
> Release-ZIP ohne Authentifizierung abrufen. Für die Installation per Repository-URL muss das
> Repo (oder ein separates Manifest-Repo bzw. GitHub Pages) öffentlich erreichbar sein.

Manuelle Installation: DLL nach `<data>/plugins/PaymentTracker_1.0.0.0/` kopieren und Jellyfin neu starten.
