**Hand­getriebene UI-Test­matrix für deinen “Calorie Tracker” (Front-End-Sicht)**  
*(ohne Code, nur Klick-/Eingabe-Szenarien – du kannst sie 1-zu-1 abarbeiten und Häkchen setzen)*  

| # | Testziel | Schritt-für-Schritt Aktion | Erwartetes Ergebnis |
|---|----------|---------------------------|---------------------|
| **A – Goal-Einstellungen** |
| A-1 | **Ziel neu anlegen (Happy Path)** | 1 → „Total Calorie Goal“ = `100 000` <br>2 → „Time Window“ = `100` <br>3 → „Start Date“ = heutiges Datum <br>4 → **Update Goal** klicken | Info-Box zeigt „Deficit of 100 000 calories over 100 days“, Daily-Target = 1000 kcal/day |
| A-2 | Ziel in **Vergangenheit** setzen | Schritt 1–2 wie oben, aber Start Date = *gestern* | Frontend blockt Save (rotes Feld oder Tooltip) **oder** Backend liefert 400 → Fehlermeldung „Start date cannot be in the past“ |
| A-3 | Ziel mit **0 kcal** | Target = `0`, übrige Felder gültig, „Update Goal“ | Validation verhindert Speichern; klare Fehlermeldung „TargetKcals must be ≠ 0“ |
| A-4 | Ziel-Fenster **> 3650 Tage** | Time Window = `4000` | Fehler: „Time Window must be between 1 and 3650 days“ |
| A-5 | **Negatives** Ziel (Überschuss) | Target = `-15 000`, Window = `30`, Start Date = Heute | Info-Box sagt „Surplus of 15 000 calories…“, Daily-Target = 500 kcal/**surplus** per day |
| **B – Daily Log Erfassung** |
| B-1 | **Neuen Tag hinzufügen (Happy Path)** | Date = heute, Burn = `2900`, Intake = `2300`, **Add Entry** | Neuer Tabellen-Eintrag, KcalsDiff = `600`, SumDiffs & Averages aktualisieren |
| B-2 | **Duplikat-Datum** eingeben | Noch einmal exakt dieselbe Date/Value-Kombi wie B-1, „Add Entry“ | Fehlermeldung („date already exists“) – kein zweiter Eintrag |
| B-3 | Datum **außerhalb** des Ziel-Fensters | Date vor Start Date oder nach `Start Date+Window` | Speichern blockiert oder 400-Fehler „Date outside goal range“ |
| B-4 | Eingabe **Burn < Intake** (Surplus-Tag) | Burn = `1800`, Intake = `2500` | KcalsDiff negativ, Zeile farblich anders oder Symbol, Gesamt-SumDiffs sinkt |
| B-5 | **Negative** Zahlen | Burn = `-2000` | Validation verhindert Eingabe (Burn ≥ 0) |
| B-6 | **Null**-Werte | Burn und Intake = `0` | KcalsDiff = 0, erlaubt? (Abhängig von Business-Regel) |
| B-7 | **Future Date** (später als heute, aber noch im Fenster) | Date = +3 Tage, Werte beliebig | Falls erlaubt: Zeile akzeptiert; andernfalls Fehlermeldung „Date cannot be in the future“ |
| **C – Bearbeiten & Löschen** |
| C-1 | **Edit** bestehender Tag | Klick „Edit“ → ändere Burn, speichere | Tabelle zeigt neue Werte, alle abhängigen Summen/Averages neu berechnet |
| C-2 | **Edit → Duplikat-Datum** | In einer Zeile Datum ändern auf bereits existierenden Tag, speichern | Fehlermeldung „duplicate date“ |
| C-3 | **Delete** Tag | Klick „Delete“, ggf. Confirm-Dialog → Ja | Zeile entfernt, Summen/Averages angepasst |
| C-4 | **Delete All Entries** Button | Klick, Confirm → Ja | Tabelle leer, Status-Box zeigt „0 of X days logged“, SumDiffs = 0 |
| **D – Aggregierte Kennzahlen** |
| D-1 | **SumDiffs laufend** | Nach jedem neuen Eintrag manuell nachrechnen (Excel/Hand): stimmt Spalte „SumDiffs“? |
| D-2 | **Avg4Days / Avg7Days** | Nach min. 7 Einträgen kontrollieren: Durchschnitt entspricht Handrechnung (auf 2 Nachkommastellen) |
| D-3 | **AvgAll** korrekt | Prüfen, ob Gesamtmittel korrekt aktualisiert wird, wenn Eintrag geändert/gelöscht |
| D-4 | **GoalDelta** Richtung | Bei Defizit-Ziel muss Wert sinken, bei Surplus-Ziel steigen (negativ werden) |
| **E – Status-Box unten** |
| E-1 | **On-Track vs Behind** | Füge Tage > Daily-Target hinzu → Status = „On schedule“ (grün) <br> Füge Tage < Daily-Target hinzu → „Behind schedule“ (rot) |
| E-2 | **Prozent-Anzeige** | Prüfe, ob Prozent = `SumDiffs / TargetKcals × 100`, korrekt gerundet |
| **F – UX/Hilfsmittel** |
| F-1 | **Reset** Button im Log-Formular | Klick → Felder werden geleert |
| F-2 | **Date-Picker format** | Lokales Format (dd.mm.yyyy) korrekt, Tastatureingabe validiert |
| F-3 | **Fokus & Tab-Reihenfolge** | Drücke Tab-Reihe durch Formular – logische Reihenfolge ohne Sprünge |
| F-4 | **Responsive Layout** | Breite < 768 px (Dev-Tools) → Karten/Tabelle stapeln sich sauber, keine horizontale Scrollbar |
| **G – Fehler-Handling (API simulieren)** |
| G-1 | **Server 500 Simulation** | Dev-Tools → Network → Block API‐Call, dann „Add Entry“ | UI zeigt Toast/Dialog „Server error – try again later“, Eingaben bleiben erhalten |
| G-2 | **Timeout** | API verzögert (Throttle), Eintrag speichern | Spinner sichtbar, UI blockiert wiederholtes Klicken, nach Timeout Fehlermeldung |

---

### So arbeitest du damit
1. Druck die Tabelle / kopiere sie in ein Test-Sheet.  
2. Gehe Zeile für Zeile durch, führe die Aktionen exakt aus.  
3. Hake „✓“ bei **erwartetem Ergebnis erreicht**, sonst „✗“ und notiere den IST-Effekt.  
4. Alle ✗‐Fälle wandern als Bug oder fehlende Business-Regel ins Backlog.

> **Tipp:** Für schnelle Regressionen kannst du die wichtigsten 5–6 Happy- & Edge-Cases (A-1, B-2, B-3, C-1, D-1, G-1) später automatisiert mit Playwright abdecken.
