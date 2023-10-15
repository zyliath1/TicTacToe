using System.Text.Json;

namespace final_work
{
    // Rajapinta tarjoaa metodit pelaajatietojen tallentamiseen ja lataamiseen.
    public interface PelaajanTallennus
    {
        // Lataa pelaajatiedot annetusta polusta.
        List<Pelaaja> LataaPelaajat(string polku);

        // Tallentaa pelaajalistauksen annettuun tiedostopolkuun.
        void TallennaPelaajat(string polku, List<Pelaaja> pelaajat);
    }

    // Toteuttaa `PelaajanTallennus` rajapinnan tiedostojen avulla.
    public class TiedostoPelaajanTallennus : PelaajanTallennus
    {
        // Lataa pelaajatiedot tiedostosta. Jos tiedostoa ei ole, palautetaan tyhjä lista.
        public List<Pelaaja> LataaPelaajat(string polku)
        {
            if (File.Exists(polku))
            {
                var sisältö = File.ReadAllText(polku);
                return JsonSerializer.Deserialize<List<Pelaaja>>(sisältö) ?? new List<Pelaaja>();
            }
            return new List<Pelaaja>();
        }

        // Serialize-pelaajalista ja tallentaa sen annettuun tiedostopolkuun.
        public void TallennaPelaajat(string polku, List<Pelaaja> pelaajat)
        {
            var päivitettySisältö = JsonSerializer.Serialize(pelaajat);
            File.WriteAllText(polku, päivitettySisältö);
        }
    }

    // Luokka pelaajien hallintaan: lataaminen, tallentaminen ja päivittäminen.
    public class PelaajaHallinta
    {
        private readonly string _tiedostoPolku;
        private readonly PelaajanTallennus _tallennus;

        // Konstruktori ottaa parametrina rajapinnan toteuttavan tallennuksen ja määrittelee tiedostopolun.
        public PelaajaHallinta(PelaajanTallennus tallennus)
        {
            _tallennus = tallennus ?? throw new ArgumentNullException(nameof(tallennus));
            _tiedostoPolku = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "pelaajat.json");
        }

        // Staattinen tietokonepelaaja; kiinteä arvo, joka ei muutu.
        public static readonly Pelaaja TietokonePelaaja = new Pelaaja
        {
            PelaajaId = Guid.Parse("00000000-0000-0000-0000-000000000000"),
            Etunimi = "Tietokone",
            Sukunimi = "",
        };

        // Lataa pelaajatiedot tiedostosta ja varmistaa, että staattinen TietokonePelaaja on aina mukana.
        public List<Pelaaja> LataaPelaajat()
        {
            var pelaajat = _tallennus.LataaPelaajat(_tiedostoPolku);

            if (!pelaajat.Any(p => p.PelaajaId == TietokonePelaaja.PelaajaId))
            {
                pelaajat.Add(TietokonePelaaja);
                TallennaPelaajat(pelaajat);
            }

            return pelaajat;
        }

        // Hakee ja palauttaa pelaajan annettujen etu- ja sukunimien perusteella.
        public Pelaaja HaePelaaja(string etunimi, string sukunimi)
        {
            var pelaajat = LataaPelaajat();
            return pelaajat.FirstOrDefault(p => p.Etunimi == etunimi && p.Sukunimi == sukunimi);
        }

        // Tallentaa annetun pelaajalistauksen tiedostoon.
        public void TallennaPelaajat(List<Pelaaja> pelaajat)
        {
            pelaajat = pelaajat ?? new List<Pelaaja>();
            _tallennus.TallennaPelaajat(_tiedostoPolku, pelaajat);
        }

        // Päivittää annetun pelaajan tiedot, tai lisää pelaajan, jos sitä ei ole jo listassa.
        public bool PäivitäTaiLisääPelaaja(Pelaaja pelaaja)
        {
            try
            {
                var pelaajat = LataaPelaajat();
                PäivitäTaiLisää(pelaajat, pelaaja);
                TallennaPelaajat(pelaajat);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Virhe pelaajan päivittämisessä: {ex.Message}");
                return false;
            }
        }

        // Apumetodi, joka päivittää pelaajan tiedot tai lisää uuden pelaajan, jos sitä ei ole jo listassa.
        private void PäivitäTaiLisää(List<Pelaaja> pelaajat, Pelaaja pelaaja)
        {
            var olemassaOlevaPelaaja = pelaajat.FirstOrDefault(p => p.PelaajaId == pelaaja.PelaajaId);
            if (olemassaOlevaPelaaja == null)
            {
                pelaajat.Add(pelaaja);
            }
            else
            {
                olemassaOlevaPelaaja.Etunimi = pelaaja.Etunimi;
                olemassaOlevaPelaaja.Sukunimi = pelaaja.Sukunimi;
                olemassaOlevaPelaaja.Syntymävuosi = pelaaja.Syntymävuosi;
                olemassaOlevaPelaaja.Voitot = pelaaja.Voitot;
                olemassaOlevaPelaaja.Tappiot = pelaaja.Tappiot;
                olemassaOlevaPelaaja.Tasapelit = pelaaja.Tasapelit;
                olemassaOlevaPelaaja.PelienYhteiskesto = pelaaja.PelienYhteiskesto;
            }
        }
    }
}