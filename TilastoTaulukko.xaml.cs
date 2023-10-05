using System.Collections.ObjectModel;

namespace final_work
{
    public partial class TilastoTaulukko : ContentPage
    {
        private PelaajaHallinta pelaajaHallinta = new PelaajaHallinta(new TiedostoPelaajanTallennus());

        public ObservableCollection<Pelaaja> PelaajaTilastot { get; set; } = new ObservableCollection<Pelaaja>();

        public TilastoTaulukko(List<Pelaaja> initialPelaajat)
        {
            InitializeComponent();
            TilastoListView.ItemsSource = PelaajaTilastot;

            if (initialPelaajat != null && initialPelaajat.Count > 0)
            {
                LataaPelaajatilastot(initialPelaajat);
            }
            else
            {
                LataaPelaajatilastot();
            }
        }

        public void P‰ivit‰PelaajaTilastot(Pelaaja pelaaja)
        {
            var existingPlayer = PelaajaTilastot.FirstOrDefault(p => p.Etunimi == pelaaja.Etunimi && p.Sukunimi == pelaaja.Sukunimi);
            if (existingPlayer != null)
            {
                existingPlayer.Voitot = pelaaja.Voitot;
                existingPlayer.Tappiot = pelaaja.Tappiot;
                existingPlayer.Tasapelit = pelaaja.Tasapelit;
                existingPlayer.PelienYhteiskesto = pelaaja.PelienYhteiskesto;
            }
            else
            {
                PelaajaTilastot.Add(pelaaja);
            }

            pelaajaHallinta.P‰ivit‰TaiLis‰‰Pelaaja(pelaaja);
        }

        private void LataaPelaajatilastot()
        {
            var pelaajat = pelaajaHallinta.LataaPelaajat();
            LataaPelaajatilastot(pelaajat);
        }

        private void LataaPelaajatilastot(List<Pelaaja> latausPelaajat)
        {
            foreach (var pelaaja in latausPelaajat)
            {
                PelaajaTilastot.Add(pelaaja);
            }
        }
    }
}