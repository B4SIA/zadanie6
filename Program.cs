using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Sqlite;

namespace ComplinexSystem
{
    public interface IEwaluowalny
    {
        string Ewaluuj();
    }

    public abstract class Osoba
    {
        public string Imie { get; }
        public string Nazwisko { get; }

        protected Osoba(string imie, string nazwisko)
        {
            Imie = imie;
            Nazwisko = nazwisko;
        }

        public virtual string PrzedstawSie()
        {
            return $"Nazywam się {Imie} {Nazwisko}.";
        }
    }

    public abstract class Pracownik : Osoba, IEwaluowalny
    {
        public string Stanowisko { get; protected set; }
        public string Dzial { get; protected set; }

        protected Pracownik(string imie, string nazwisko, string stanowisko, string dzial)
            : base(imie, nazwisko)
        {
            Stanowisko = stanowisko;
            Dzial = dzial;
        }

        public abstract string OpisObowiazkow();
        public abstract string Ewaluuj();
    }

    public class InzynierProdukcji : Pracownik
    {
        public InzynierProdukcji(string imie, string nazwisko)
            : base(imie, nazwisko, "Inżynier Produkcji", "Produkcja") { }

        public override string OpisObowiazkow() => "Projektowanie i optymalizacja procesów produkcyjnych.";
        public override string Ewaluuj() => "Ocena efektywności projektów i innowacji technologicznych.";
    }

    public class OperatorMaszyn : Pracownik
    {
        public OperatorMaszyn(string imie, string nazwisko)
            : base(imie, nazwisko, "Operator Maszyn", "Produkcja") { }

        public override string OpisObowiazkow() => "Obsługa i konserwacja maszyn produkcyjnych.";
        public override string Ewaluuj() => "Ocena jakości i terminowości pracy.";
    }

    public class KontrolerJakosci : Pracownik
    {
        public KontrolerJakosci(string imie, string nazwisko)
            : base(imie, nazwisko, "Kontroler Jakości", "Kontrola Jakości") { }

        public override string OpisObowiazkow() => "Monitorowanie jakości produktów i procesów.";
        public override string Ewaluuj() => "Ocena zgodności z normami jakości.";
    }

    public class PrzedstawicielHandlowy : Pracownik
    {
        public PrzedstawicielHandlowy(string imie, string nazwisko)
            : base(imie, nazwisko, "Przedstawiciel Handlowy", "Sprzedaż") { }

        public override string OpisObowiazkow() => "Pozyskiwanie klientów i sprzedaż produktów.";
        public override string Ewaluuj() => "Ocena wyników sprzedażowych i relacji z klientami.";
    }

    public class SpecjalistaHR : Pracownik
    {
        public SpecjalistaHR(string imie, string nazwisko)
            : base(imie, nazwisko, "Specjalista HR", "Zasoby Ludzkie") { }

        public override string OpisObowiazkow() => "Rekrutacja, szkolenia i rozwój pracowników.";
        public override string Ewaluuj() => "Ocena efektywności działań kadrowych.";
    }

    public class KierownikProjektu : Pracownik
    {
        public KierownikProjektu(string imie, string nazwisko)
            : base(imie, nazwisko, "Kierownik Projektu", "Zarządzanie") { }

        public override string OpisObowiazkow() => "Zarządzanie projektem i zespołem.";
        public override string Ewaluuj() => "Ocena zarządzania zespołem i realizacji celów.";
    }

    public class Projekt
    {
        public string Nazwa { get; }
        public KierownikProjektu Kierownik { get; }
        public List<Pracownik> Zespol { get; }

        public Projekt(string nazwa, KierownikProjektu kierownik)
        {
            Nazwa = nazwa;
            Kierownik = kierownik;
            Zespol = new List<Pracownik> { kierownik };
        }

        public void DodajDoZespolu(Pracownik pracownik)
        {
            if (!Zespol.Contains(pracownik))
                Zespol.Add(pracownik);
        }

        public void DodajDoZespolu(List<Pracownik> pracownicy)
        {
            foreach (var pracownik in pracownicy)
            {
                DodajDoZespolu(pracownik);
            }
        }
    }

    public class RejestrPracownikow
    {
        private string connectionString = "Data Source=complinex.db";

        public RejestrPracownikow()
        {
            using var con = new SqliteConnection(connectionString);
            con.Open();

            var tableCmd = con.CreateCommand();
            tableCmd.CommandText =
            @"CREATE TABLE IF NOT EXISTS Pracownicy (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Imie TEXT NOT NULL,
                Nazwisko TEXT NOT NULL,
                Stanowisko TEXT NOT NULL,
                Dzial TEXT NOT NULL
            );";
            tableCmd.ExecuteNonQuery();
        }

        public void Dodaj(Pracownik p)
        {
            using var con = new SqliteConnection(connectionString);
            con.Open();

            var insertCmd = con.CreateCommand();
            insertCmd.CommandText =
            @"INSERT INTO Pracownicy (Imie, Nazwisko, Stanowisko, Dzial) 
            VALUES ($imie, $nazwisko, $stanowisko, $dzial);";
            insertCmd.Parameters.AddWithValue("$imie", p.Imie);
            insertCmd.Parameters.AddWithValue("$nazwisko", p.Nazwisko);
            insertCmd.Parameters.AddWithValue("$stanowisko", p.Stanowisko);
            insertCmd.Parameters.AddWithValue("$dzial", p.Dzial);

            insertCmd.ExecuteNonQuery();
        }

        public IEnumerable<Pracownik> PobierzWszystkichPracownikow()
        {
            var pracownicy = new List<Pracownik>();

            using var con = new SqliteConnection(connectionString);
            con.Open();

            var selectCmd = con.CreateCommand();
            selectCmd.CommandText =
            "SELECT Imie, Nazwisko, Stanowisko, Dzial FROM Pracownicy;";

            using var reader = selectCmd.ExecuteReader();
            while (reader.Read())
            {
                string imie = reader.GetString(0);
                string nazwisko = reader.GetString(1);
                string stanowisko = reader.GetString(2);
                string dzial = reader.GetString(3);

                Pracownik pracownik = stanowisko switch
                {
                    "Inżynier Produkcji" => new InzynierProdukcji(imie, nazwisko),
                    "Operator Maszyn" => new OperatorMaszyn(imie, nazwisko),
                    "Kontroler Jakości" => new KontrolerJakosci(imie, nazwisko),
                    "Przedstawiciel Handlowy" => new PrzedstawicielHandlowy(imie, nazwisko),
                    "Specjalista HR" => new SpecjalistaHR(imie, nazwisko),
                    "Kierownik Projektu" => new KierownikProjektu(imie, nazwisko),
                    _ => throw new Exception($"Nieznane stanowisko: {stanowisko}")
                };

                pracownicy.Add(pracownik);
            }

            return pracownicy;
        }

        public IEnumerable<Pracownik> WyszukajPoStanowisku(string stanowisko)
        {
            if (string.IsNullOrWhiteSpace(stanowisko)) return Enumerable.Empty<Pracownik>();
            return PobierzWszystkichPracownikow().Where(p => p.Stanowisko.Equals(stanowisko, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<Pracownik> WyszukajPoDziale(string dzial)
        {
            if (string.IsNullOrWhiteSpace(dzial)) return Enumerable.Empty<Pracownik>();
            return PobierzWszystkichPracownikow().Where(p => p.Dzial.Equals(dzial, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<Pracownik> WyszukajPoKompetencjach(string kompetencje)
        {
            if (string.IsNullOrWhiteSpace(kompetencje)) return Enumerable.Empty<Pracownik>();
            return PobierzWszystkichPracownikow().Where(p => p.OpisObowiazkow().Contains(kompetencje, StringComparison.OrdinalIgnoreCase));
        }
    }


    class Program
    {
        static void Main()
        {
            var rejestr = new RejestrPracownikow();

            var pracownik1 = new InzynierProdukcji("Jan", "Kowalski");
            var pracownik2 = new OperatorMaszyn("Anna", "Nowak");
            var pracownik3 = new KontrolerJakosci("Piotr", "Wojciechowski");
            var pracownik4 = new PrzedstawicielHandlowy("Katarzyna", "Pawlak");
            var pracownik5 = new SpecjalistaHR("Marek", "Zawisza");
            var pracownik6 = new KierownikProjektu("Michał", "Kaczmarek");

            rejestr.Dodaj(pracownik1);
            rejestr.Dodaj(pracownik2);
            rejestr.Dodaj(pracownik3);
            rejestr.Dodaj(pracownik4);
            rejestr.Dodaj(pracownik5);
            rejestr.Dodaj(pracownik6);

            while (true)
            {
                Console.WriteLine("\nWybierz opcję:");
                Console.WriteLine("1. Dodaj pracownika");
                Console.WriteLine("2. Znajdź pracownika");
                Console.WriteLine("3. Wyświetl wszystkich pracowników");
                Console.WriteLine("4. Zakończ");
                string? wybor = Console.ReadLine();

                if (wybor == "1")
                {
                    Console.WriteLine("Dodaj nowego pracownika:");
                    Console.Write("Imię: ");
                    string? imie = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(imie))
                    {
                        Console.WriteLine("Imię nie może być puste!");
                        continue;
                    }
                    Console.Write("Nazwisko: ");
                    string? nazwisko = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(nazwisko))
                    {
                        Console.WriteLine("Nazwisko nie może być puste!");
                        continue;
                    }
                    Console.WriteLine("Wybierz stanowisko:");
                    Console.WriteLine("1. Inżynier Produkcji\n2. Operator Maszyn\n3. Kontroler Jakości\n4. Przedstawiciel Handlowy\n5. Specjalista HR\n6. Kierownik Projektu");

                    Pracownik? nowyPracownik = null;
                    string? stanowisko = Console.ReadLine();
                    switch (stanowisko)
                    {
                        case "1":
                            nowyPracownik = new InzynierProdukcji(imie, nazwisko);
                            break;
                        case "2":
                            nowyPracownik = new OperatorMaszyn(imie, nazwisko);
                            break;
                        case "3":
                            nowyPracownik = new KontrolerJakosci(imie, nazwisko);
                            break;
                        case "4":
                            nowyPracownik = new PrzedstawicielHandlowy(imie, nazwisko);
                            break;
                        case "5":
                            nowyPracownik = new SpecjalistaHR(imie, nazwisko);
                            break;
                        case "6":
                            nowyPracownik = new KierownikProjektu(imie, nazwisko);
                            break;
                        default:
                            Console.WriteLine("Nieprawidłowy wybór!");
                            continue;
                    }

                    rejestr.Dodaj(nowyPracownik);
                    Console.WriteLine("Pracownik dodany!");
                }
                else if (wybor == "2")
                {
                    Console.WriteLine("\nWybierz opcję wyszukiwania:");
                    Console.WriteLine("1. Wyszukaj po stanowisku");
                    Console.WriteLine("2. Wyszukaj po dziale");
                    Console.WriteLine("3. Wyszukaj po kompetencjach");
                    string? opcjaWyszukiwania = Console.ReadLine();

                    IEnumerable<Pracownik> ?znalezieniPracownicy = null;

                    if (opcjaWyszukiwania == "1")
                    {
                        Console.Write("Podaj stanowisko: ");
                        string? stanowisko = Console.ReadLine();
                        if (string.IsNullOrWhiteSpace(stanowisko))
                        {
                            Console.WriteLine("Stanowisko nie może być puste!");
                            continue;
                        }
                        znalezieniPracownicy = rejestr.WyszukajPoStanowisku(stanowisko);
                    }
                    else if (opcjaWyszukiwania == "2")
                    {
                        Console.Write("Podaj dział: ");
                        string? dzial = Console.ReadLine();
                        if (string.IsNullOrWhiteSpace(dzial))
                        {
                            Console.WriteLine("Dział nie może być pusty!");
                            continue;
                        }
                        znalezieniPracownicy = rejestr.WyszukajPoDziale(dzial);
                    }
                    else if (opcjaWyszukiwania == "3")
                    {
                        Console.Write("Podaj kompetencje: ");
                        string? kompetencje = Console.ReadLine();
                        if (string.IsNullOrWhiteSpace(kompetencje))
                        {
                            Console.WriteLine("Kompetencje nie mogą być puste!");
                            continue;
                        }
                        znalezieniPracownicy = rejestr.WyszukajPoKompetencjach(kompetencje);
                    }
                    else
                    {
                        Console.WriteLine("Nieprawidłowy wybór!");
                        continue;
                    }

                    Console.WriteLine("Znalezieni pracownicy:");
                    foreach (var pracownik in znalezieniPracownicy)
                    {
                        Console.WriteLine($"{pracownik.PrzedstawSie()} - {pracownik.OpisObowiazkow()} Ocena: {pracownik.Ewaluuj()}");
                    }
                }
                else if (wybor == "3")
                {
                    var wszyscyPracownicy = rejestr.PobierzWszystkichPracownikow();
                    Console.WriteLine("\nWszyscy pracownicy:");
                    foreach (var pracownik in wszyscyPracownicy)
                    {
                        Console.WriteLine($"{pracownik.PrzedstawSie()} - {pracownik.OpisObowiazkow()} {pracownik.Ewaluuj()}");
                    }
                }
                else if (wybor == "4")
                {
                    break;
                }
                else
                {
                    Console.WriteLine("Nieprawidłowy wybór!");
                }
            }
        }
    }
}
