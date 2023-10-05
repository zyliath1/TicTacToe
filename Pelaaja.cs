using System.Text.Json.Serialization;

namespace final_work
{
    public class Pelaaja
    {
        public Guid PelaajaId { get; set; } = Guid.NewGuid();
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
        public enum PelaajaTyyppi
        {
            Ihminen,
            Tietokone
        }

        public void LisääPeliaika(TimeSpan kesto)
        {
            PelienYhteiskesto += kesto;
        }
    }
}