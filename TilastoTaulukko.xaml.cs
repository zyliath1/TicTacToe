using System.Collections.ObjectModel;

namespace final_work
{
    public partial class TilastoTaulukko : ContentPage
    {
        // PelaajaHallinta-instanssi tiedostoon tallennusta ja latausta varten.
        private PelaajaHallinta pelaajaHallinta = new PelaajaHallinta(new TiedostoPelaajanTallennus());

        // Kokoelma, joka pit‰‰ kirjaa pelaajien tilastoista ja p‰ivitt‰‰ ne automaattisesti.
        public ObservableCollection<Pelaaja> PelaajaTilastot { get; set; } = new ObservableCollection<Pelaaja>();

        // Konstruktori, joka alustaa tilastotaulukon sivun ja lataa pelaajatilastot.
        public TilastoTaulukko(List<Pelaaja> listallaOlevatPelaajat)
        {
            InitializeComponent();
            TilastoListView.ItemsSource = PelaajaTilastot;

            // Lataa pelaajatilastot joko annetusta listasta tai tiedostosta.
            if (listallaOlevatPelaajat != null && listallaOlevatPelaajat.Count > 0)
            {
                LataaPelaajatilastot(listallaOlevatPelaajat);
            }
            else
            {
                LataaPelaajatilastot();
            }
        }

        // P‰ivitt‰‰ annetun pelaajan tilastot kokoelmassa ja tallentaa muutokset tiedostoon.
        public void P‰ivit‰PelaajaTilastot(Pelaaja pelaaja)
        {
            var nykyinenPelaaja = PelaajaTilastot.FirstOrDefault(p => p.Etunimi == pelaaja.Etunimi && p.Sukunimi == pelaaja.Sukunimi);
            if (nykyinenPelaaja != null)
            {
                nykyinenPelaaja.Voitot = pelaaja.Voitot;
                nykyinenPelaaja.Tappiot = pelaaja.Tappiot;
                nykyinenPelaaja.Tasapelit = pelaaja.Tasapelit;
                nykyinenPelaaja.PelienYhteiskesto = pelaaja.PelienYhteiskesto;
            }
            else
            {
                PelaajaTilastot.Add(pelaaja);
            }

            pelaajaHallinta.P‰ivit‰TaiLis‰‰Pelaaja(pelaaja);
        }

        // Lataa pelaajatilastot tiedostosta.
        private void LataaPelaajatilastot()
        {
            var pelaajat = pelaajaHallinta.LataaPelaajat();
            LataaPelaajatilastot(pelaajat);
        }

        // P‰ivitt‰‰ pelaajatilastot kokoelmaa annetulla pelaajalistan perusteella.
        private void LataaPelaajatilastot(List<Pelaaja> latausPelaajat)
        {
            foreach (var pelaaja in latausPelaajat)
            {
                PelaajaTilastot.Add(pelaaja);
            }
        }
    }
}