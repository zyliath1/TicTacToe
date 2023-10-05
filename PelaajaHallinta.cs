using System.Text.Json;

namespace final_work
{
    public interface PelaajanTallennus
    {
        List<Pelaaja> LataaPelaajat(string polku);
        void TallennaPelaajat(string polku, List<Pelaaja> pelaajat);
    }

    public class TiedostoPelaajanTallennus : PelaajanTallennus
    {
        public List<Pelaaja> LataaPelaajat(string polku)
        {
            if (File.Exists(polku))
            {
                var sisalto = File.ReadAllText(polku);
                return JsonSerializer.Deserialize<List<Pelaaja>>(sisalto) ?? new List<Pelaaja>();
            }
            return new List<Pelaaja>();
        }



        public void TallennaPelaajat(string polku, List<Pelaaja> pelaajat)
        {
            var paivitettySisalto = JsonSerializer.Serialize(pelaajat);
            File.WriteAllText(polku, paivitettySisalto);
        }
    }

    public class PelaajaHallinta
    {
        private readonly string _tiedostoPolku;
        private readonly PelaajanTallennus _tallennusPalvelu;

        public PelaajaHallinta(PelaajanTallennus tallennusPalvelu)
        {
            _tallennusPalvelu = tallennusPalvelu ?? throw new ArgumentNullException(nameof(tallennusPalvelu));
            _tiedostoPolku = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "pelaajat.json");
        }

        public List<Pelaaja> LataaPelaajat()
        {
            return _tallennusPalvelu.LataaPelaajat(_tiedostoPolku);
        }

        public Pelaaja HaePelaaja(string etunimi, string sukunimi)
        {
            var pelaajat = LataaPelaajat();
            return pelaajat.FirstOrDefault(p => p.Etunimi == etunimi && p.Sukunimi == sukunimi);
        }

        public void TallennaPelaajat(List<Pelaaja> pelaajat)
        {
            pelaajat = pelaajat ?? new List<Pelaaja>();
            _tallennusPalvelu.TallennaPelaajat(_tiedostoPolku, pelaajat);
        }

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
                // Tässä voisi olla virhelogitus sen sijaan, että tulostetaan suoraan konsoliin.
                Console.WriteLine($"Virhe pelaajan päivittämisessä: {ex.Message}");
                return false;
            }
        }

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


