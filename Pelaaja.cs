using System.ComponentModel;

namespace final_work
{
    // Edustaa pelissä osallistuvaa pelaajaa, joka voi olla joko ihminen tai tietokone.
    public class Pelaaja : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Guid PelaajaId { get; set; } = Guid.NewGuid(); // Yksilöllinen tunniste pelaajalle
        public string Etunimi { get; set; }
        public string Sukunimi { get; set; }
        public int Syntymävuosi { get; set; }
        public PelaajaTyyppi Tyyppi { get; set; }
        public int Voitot { get; set; }
        public int Tappiot { get; set; }
        public int Tasapelit { get; set; }
        public TimeSpan PelienYhteiskesto { get; set; }

        public string MuotoiltuPelienYhteiskesto
        {
            get
            {
                return PelienYhteiskesto.ToString(@"hh\:mm\:ss");
            }
        }

        // Enum PelaajaTyyppi, joka määrittää onko pelaaja ihminen vai tietokone.
        public enum PelaajaTyyppi
        {
            Ihminen,
            Tietokone
        }

        // Lisää peliaikaa pelaajalle pelin loputtua.
        public void LisääPeliaika(TimeSpan kesto)
        {
            PelienYhteiskesto += kesto;
            OnPropertyChanged(nameof(PelienYhteiskesto));
            OnPropertyChanged(nameof(MuotoiltuPelienYhteiskesto));
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}