
namespace final_work
{
    public class PelinTilanne
    {
        public Pelaaja Player1 { get; set; }
        public Pelaaja Player2 { get; set; }
        public string[,] Pelilauta { get; private set; } = new string[3, 3];
        public Pelaaja Vuorossa { get; private set; }

        public PelinTilanne(Pelaaja pelaaja1, Pelaaja pelaaja2)
        {
            Player1 = pelaaja1;
            Player2 = pelaaja2;
            Vuorossa = pelaaja1;
        }

        public void TeeSiirto(int x, int y)
        {
            if (string.IsNullOrEmpty(Pelilauta[x, y]))
            {
                Pelilauta[x, y] = Vuorossa == Player1 ? "X" : "O";
                VaihdaVuoro();
            }
        }

        public bool OnkoRuudussaMerkki(int x, int y)
        {
            return !string.IsNullOrEmpty(Pelilauta[x, y]);
        }

        private void VaihdaVuoro()
        {
            Vuorossa = Vuorossa == Player1 ? Player2 : Player1;
        }

        public bool OnkoVoittaja()
        {
            return OnkoRiviTaiSarakeSama(0) || OnkoRiviTaiSarakeSama(1) || OnkoRiviTaiSarakeSama(2)
                || OnkoDiagonaaliSama();
        }

        private bool OnkoRiviTaiSarakeSama(int index)
        {
            return (!string.IsNullOrEmpty(Pelilauta[index, 0]) && Pelilauta[index, 0] == Pelilauta[index, 1] && Pelilauta[index, 1] == Pelilauta[index, 2])
                || (!string.IsNullOrEmpty(Pelilauta[0, index]) && Pelilauta[0, index] == Pelilauta[1, index] && Pelilauta[1, index] == Pelilauta[2, index]);
        }

        private bool OnkoDiagonaaliSama()
        {
            return (!string.IsNullOrEmpty(Pelilauta[0, 0]) && Pelilauta[0, 0] == Pelilauta[1, 1] && Pelilauta[1, 1] == Pelilauta[2, 2])
                || (!string.IsNullOrEmpty(Pelilauta[0, 2]) && Pelilauta[0, 2] == Pelilauta[1, 1] && Pelilauta[1, 1] == Pelilauta[2, 0]);
        }

        public bool OnkoTasapeli()
        {
            if (OnkoVoittaja()) return false;

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (string.IsNullOrEmpty(Pelilauta[i, j])) return false;
                }
            }
            return true;
        }
    }
}

