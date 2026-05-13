# APLIKACJA W PROCESIE TWORZENIA, PONIŻEJ OPIS TEORETYCZNY

# System zgłaszania usterek w firmie / akademiku

System zgłaszania usterek to aplikacja webowa przeznaczona dla firm lub akademików,
umożliwiająca sprawne zarządzanie zgłoszeniami awarii i usterek. System obsługuje trzech typów
użytkowników: zwykłych użytkowników (lokatorów lub pracowników), techników odpowiedzialnych
za realizację napraw oraz administratorów systemu.

Głównym celem systemu jest uproszczenie i przyspieszenie procesu od momentu zgłoszenia usterki
do jej rozwiązania. Użytkownik zgłasza problem przez interfejs WWW, określając jego kategorię i
opis. Technik otrzymuje zgłoszenie, zmienia jego status w miarę postępów i dodaje komentarze
techniczne. Administrator nadzoruje cały system, zarządza użytkownikami i generuje raporty.

## Funkcjonalności

Użytkownik zwykły może:

- Logować się do systemu
- Edytować własny profilu
- Zgłaszać nowe usterki (tytuł, opis, lokalizacja, priorytet)
- Przeglądać swoje zgłoszeń
- Sprawdzać status zgłoszenia

Możliwości technika:

- Logowanie się do systemu
- Przeglądanie przydzielonych zgłoszeń
- Zmiana statusu naprawy (np. Nowe, w trakcie, zakończone)
- Dziedziczy możliwości użytkownika
- Dodawanie komentarzy technicznych do zgłoszeń
- Przeglądanie historii napraw
- Filtrowanie zgłoszeń (wg statusu, priorytetu, daty)
- Generowanie raportów z napraw

Administrator posiada uprawnienia:

- Pełny dostęp do funkcji technika
- Zarządzanie użytkownikami (dodawanie, edycja, usuwanie, role)
- Przydzielanie zgłoszeń do techników
- Przeglądanie wszystkich zgłoszeń w systemie
- Generowanie raportów systemowych
- Konfiguracja systemu (priorytety, kategorie usterek)

Ogólne funkcje systemu:

- System priorytetów zgłoszeń (Niski / Średnio / Wysoki / Krytyczny)
- Historia zmian statusu zgłoszenia
- System komentarzy i komunikacji wewnątrz zgłoszenia
- Autoryzacja i uwierzytelnianie użytkowników


System rozróżnia trzy role: Użytkownik zwykły ma dostęp wyłącznie do własnych zgłoszeń. Technik
widzi zgłoszenia mu przydzielone i może nimi zarządzać. Administrator posiada pełną kontrolę nad
systemem, użytkownikami i raportami.