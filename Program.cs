using Blackjack.Comms;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Blackjack
{
    class Program
    {
        //Huvudmetoden
        static void Main(string[] args)
        {
            Program.AllocConsole();//Startar console
            Game game = new(1, 2); //initierar ett object av classen Game

        }
        //Metod f�r consolen
        private void Form1_Load(object sender, EventArgs e)
        {
            AllocConsole();
        }
        //G�r n�got med consolen
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();
    }

    /// <summary>
    /// Sj�lva spelet
    /// </summary>
    public class Game
    {
        //Array f�r att h�lla alla spelare
        readonly Player[] players;
        //Kortleken
        readonly Deck deck;
        //Servern
        public Communication_server server;
        //Konstrukt�rmetoden, kallas n�r objectet instantieras
        //Metoden skapar servern och v�ntar tills den �r f�rdig
        //Samt skapar upp spelare och en kortlek
        //Tillsist startar den spel loopen
        public Game(int amount_player, int decks)
        {
            server = new();
            while (!server.done) { }
            Console.WriteLine("continued");
            players = this.Player_creater(amount_player);
            deck = new Deck(decks);
            this.Loop();
        }

        //Skapar upp alla spelar object
        //�ven Ai
        private Player[] Player_creater(int amount)
        {
            Player[] temp = new Player[amount + 1];
            Player.server = server;

            for (int i = 0; i < (temp.Length - 1); i++)
            {
                temp[i] = new Player("Player " + (i + 1));
            }

            temp[^1] = new Ai("AI");

            return (temp);
        }

        /// <summary>
        /// Avg�r vem som har vunnit i slutet genom att loopa genom hela arrayen med spelare
        /// Kollar p� summan av varje spelares hand med hj�lp av sum() metoden
        /// </summary>
        /// <returns>
        /// Str�ng, Tillbaka ger den en str�ng som �r namnet p� vinnaren (Ai/Player 1)
        /// </returns>
        private String Check_winner()
        {
            int winner_sum = 0;
            string winner = "";
            bool all_bust = true;

            foreach (Player i in players)
            {
                if (i.Sum() > winner_sum && i.Sum() < 22)
                {
                    winner_sum = i.Sum();
                    winner = i.name;
                }

                if (i.Sum() < 22)
                {
                    all_bust = false;
                }
            }

            if (all_bust)
            {
                return (players[^1].name);
            }

            return (winner);
        }

        /// <summary>
        /// Kallar p� print_hand() metoden hos varje spelare.
        /// Vilket resulterar i att varje printas ut hos client
        /// </summary>
        private void Print_hands()
        {
            foreach (Player i in this.players)
            {
                i.Print_hand();
            }
        }
        /// <summary>
        /// Kallar p� Take_card(D�ck) metoden hos varje spelare
        /// Vilket resulterar i att varje spelare tar ett kort i taget
        /// </summary>
        private void Start()
        {
            for (int j = 0; j < 2; j++)
            {
                foreach (Player i in this.players)
                {
                    i.Take_card(deck);
                }
            }
        }
        /// <summary>
        /// Sj�lva spelet
        /// Best�r av en loop som bryts n�r ingen av spelarna vill ha ett kort
        /// Eller n�r n�gon av spelarna har g�tt �ver 21 po�ng
        /// Printar f�rst ut h�nderna med hj�lp av Print_hands()
        /// Sedan loopar den genom varje spelar och fr�gar om hen vill ha ett kort
        /// Med hj�lp av Wants_card(Deck) metoden
        /// </summary>
        private void Mid()
        {
            bool loop = true;
            while (loop)
            {
                loop = false;
                this.Print_hands();
                foreach (Player i in this.players)
                {
                    if (i.wants)
                    {
                        i.Wants_card(this.deck);
                    }
                    if (i.bust)
                    {
                        loop = false;
                        break;
                    }
                    if (i.wants)
                    {
                        loop = true;
                    }
                }
            }
            Console.WriteLine("loop done");
        }
        /// <summary>
        /// Kollar med hj�lp av metoden Check_winner() vem som har vunnit
        /// Skickar det sedan till clienten
        /// </summary>
        private void End()
        {
            //Print winner name on screen using Check_winner()
            Console.Write(this.Check_winner());
            server.SendWinner(this.Check_winner());
        }
        /// <summary>
        /// Metod f�r att kalla p� de olika stegen i spelet
        /// Start()
        /// Mid()
        /// End()
        /// </summary>
        private void Loop()
        {
            this.Start();
            this.Mid();
            this.End();
        }

    }

    /// <summary>
    /// Sj�lva kortet
    /// </summary>
    public class Card
    {
        public string color;
        public int value;
        public int blackjackValue;
        /// <summary>
        /// Ger kortet dess olika v�rden
        /// </summary>
        /// <param name="_color">Dess "f�rg" Deck klassen kommer ge den Club, Heart, Diamond or Spade, som str�ng</param> 
        /// <param name="_value">Dess vanliga v�rde. Allts� ess till kung, som int</param>
        /// <param name="_blackjackValue">Dess v�rde i blackjack. Ess b�rjar med v�rdet 11, 10 till kung �r v�rda 10 resterande kort har value blackjackvalue</param>
        public Card(string _color, int _value, int _blackjackValue)
        {
            color = _color;
            value = _value;
            blackjackValue = _blackjackValue;
        }
        /// <summary>
        /// Metod f�r att byta v�rdet p� ess, det den g�r �r att den kollar att det �r ett ess
        /// Och sedan byter den dess blackjackvalue till 1 om den var 11 eller tv�rtom
        /// </summary>
        public void SwitchAce()
        {
            if (this.value == 1)
            {
                this.blackjackValue = (this.blackjackValue == 1) ? 11 : 1;
            }
        }


    }

    /// <summary>
    /// Kortleks klassen
    /// F�rvarar alla korten i en lista
    /// </summary>
    public class Deck
    {
        public List<Card> deck = new();
        private Random rand = new();
        /// <summary>
        /// Kallar p� metoden f�r att fylla kortleken med kort
        /// och sedan metoden f�r att blanda den
        /// </summary>
        /// <param name="amount">
        /// Int, avg�r hur m�nga kortlekar det ska vara
        /// </param>
        public Deck(int amount)
        {
            this.deck = DeckCreater(amount);
            this.Shuffle();
        }

        /// <summary>
        /// Instantierar kort och appendar dem sedan till en tempor�r lista,
        /// som h�ller korten i metoden.
        /// Den g�r detta genom att loopa genom varje steg i arrayen colors
        /// Arrayen avg�r sedan vilken f�rg det ska vara p� korten som skapas
        /// D�refter loopar den tretton varv och ger kortet varvnumret till kortets value och blackjackvalue
        /// Undantaget �r f�r knekt och upp�t som ist�llet f�r blackjackvalue 10
        /// Ess instantieras separat med dess specifika v�rden
        /// </summary>
        /// <param name="amount">
        /// Int, Avg�r hur m�nga varv yttersta loopen ska g�
        /// och d�rmed hur m�nga kortleks m�ngder av kort som ska instantieras
        /// </param>
        /// <returns>
        /// List<Card>, Returnera en lista med alla kort objekten
        /// </returns>
        public static List<Card> DeckCreater(int amount)
        {
            var temp_deck = new List<Card>();
            string[] colors = new string[] { "Heart", "Club", "Diamond", "Spade" };

            //Appends all cards to array
            for (int h = 0; h < amount; h++)
                foreach (string i in colors)
                {
                    temp_deck.Add(new Card(i, 1, 11));
                    for (int j = 2; j < 13; j++)
                    {
                        if (j > 10)
                        {
                            temp_deck.Add(new Card(i, j, 10));
                            continue;
                        }
                        temp_deck.Add(new Card(i, j, j));
                    }
                }
            //Return array of all cards
            return (temp_deck);
        }
        /// <summary>
        /// Blandar runt korten genom att byta plats p� tv� kort
        /// tills alla kort i kortleken har bytt plats
        /// </summary>
        private void Shuffle()
        {
            for (int i = 0; i < 1; i++)
            {
                for (int j = this.deck.Count - 1; j > 0; j--)
                {
                    int k = rand.Next(j + 1);
                    Card temp = this.deck[j];
                    this.deck[j] = this.deck[k];
                    this.deck[k] = temp;
                }
            }
        }
        /// <summary>
        /// Sparar kortet tempor�rt
        /// och tar bort det fr�n deck listan
        /// </summary>
        /// <returns>
        /// Card, Returnerar ett kort
        /// </returns>
        public Card GiveCard()
        {
            Card temp = deck[0];
            deck.RemoveAt(0);

            return (temp);
        }
    }
    /// <summary>
    /// Spelar klassen
    /// Har en lista f�r att h�lla i korten
    /// och en server variabel f�r att kunna kalla p� metoder hos servern
    /// Det �r denna klass som spelaren interagerar genom
    /// </summary>
    public class Player
    {
        public List<Card> hand = new List<Card>();
        public string name;
        public bool wants = true;
        public bool bust = false;
        public static Communication_server server;
        /// <summary>
        /// Ger spelaren dess namn
        /// </summary>
        /// <param name="_name">
        /// Str�ng, Ska helst vara ett namn
        /// </param>
        public Player(string _name)
        {
            name = _name;
        }
        /// <summary>
        /// Anv�nds f�r att printa ut spelarens hand till clienten
        /// Anv�ndes servens SendHand(Str�ng) metod
        /// Som argument passerar den Hand_to_code() metoden
        /// Som omvandlar spelarens hand till en str�ng som clienten kan tolka.
        /// </summary>
        public void Print_hand()
        {

            server.SendHand(this.Hand_to_code());

            /*
            Console.WriteLine(this.name);
            foreach (Card i in this.hand)
            {
                Console.WriteLine(i.value);
            }
            Console.WriteLine(this.Sum());
            Console.WriteLine("");
            */
        }
        /// <summary>
        /// Loopar genom alla kort i spelarens hand och omvandlar varje f�rg till en bokstav
        /// omvandlar ocks� dess v�rde till en siffra eller bokstav
        /// Anv�nder en switch case f�r att enkelt checka av alla de m�jliga varianterna
        /// I slutet p� str�ngen hamnar ocks� summan av spelarens hand
        /// </summary>
        /// <returns>
        /// Str�ng, Returnerar en str�ng som servern enkelt kan skicka iv�g till clienten
        /// som clienten sedan enkelt kan tolka
        /// </returns>
        private String Hand_to_code()
        {
            String code = " ";
            foreach (Card i in this.hand)
            {
                switch (i.color)
                {
                    case "Spade":
                        code += "s";
                        break;
                    case "Heart":
                        code += "h";
                        break;
                    case "Club":
                        code += "c";
                        break;
                    case "Diamond":
                        code += "d";
                        break;
                }

                switch (i.value)
                {
                    case 1:
                        code += "a";
                        break;
                    case 13:
                        code += "k";
                        break;
                    case 12:
                        code += "q";
                        break;
                    case 11:
                        code += "j";
                        break;
                    case 10:
                        code += "t";
                        break;
                    default:
                        code += i.value;
                        break;
                }
            }
            code += " ";
            code += this.Sum();
            return code;
        }
        /// <summary>
        /// Tar kort fr�n kortleken och appendar det till spelarens lista
        /// som symboliserar spelarens hand
        /// Kollar �ven om essen beh�ver byta v�rde
        /// </summary>
        /// <param name="deck">
        /// Argumentet �r Deck allts� en referens till kortleken
        /// s� att den vet vad den ska interagera med
        /// </param>
        public void Take_card(Deck deck)
        {
            //Adds card to hand
            this.hand.Add(deck.GiveCard());
            this.Check_ace();
        }

        /// <summary>
        /// Kollar om spelaren vill ha ett kort
        /// F�rsta kravet �r att spelaren har mindre �n 21 po�ng
        /// Om hen inte har det f�rlorar spelaren
        /// Annars fr�gar den spelaren om hen vill ha ett kort via servern
        /// genom AskForInput() metoden
        /// Den v�ntar p� den metoden till svaret �r y || n
        /// Om svaret blir y kallar den p� Take_card(Deck) metoden
        /// Annars s�tter den bool variabeln wants till false
        /// Detta leder till att spelaren inte fr�gas igen
        /// </summary>
        /// <param name="deck">
        /// Argumentet �r kortleken s� att det sedan kan anv�ndas som argument i Take_card(Deck)
        /// </param>
        public virtual void Wants_card(Deck deck)
        {
            if (this.Sum() < 21)
            {
                String confirmation = null;
                while (confirmation != "y" && confirmation != "n")
                {
                    confirmation = Player.server.AskForInput();
                }
                if (confirmation == "y")
                {
                    this.Take_card(deck);
                }
                else
                {
                    wants = false;
                }
                confirmation = null;
            }
            else
            {
                bust = true;
            }
        }

        /// <summary>
        /// Kollar om esset beh�ver byta v�rde
        /// Kravet �r att spelaren har mer �n 21 po�ng
        /// Sedan letar den upp f�rsta esset som �r v�rt 11 i spelarhanden
        /// och anv�nder dess SwitchAce() metod
        /// </summary>
        private void Check_ace()
        {
            if (this.Sum() > 21)
            {
                foreach (Card i in this.hand)
                {
                    if (i.blackjackValue == 11)
                    {
                        i.SwitchAce();
                        break;
                    }
                }
            }
        }
        /// <summary>
        /// R�knar ut v�rdet p� spelarhanden
        /// Den g�r det genom att iterera genom alla kort i spelarlistan
        /// och sedan l�gger till dess blackjackvalue till en summa variabel
        /// </summary>
        /// <returns>
        /// Int, Returnerar summan av spelarens hand
        /// </returns>
        public int Sum()
        {
            int output = 0;
            foreach (Card i in hand)
            {
                output += i.blackjackValue;
            }
            return (output);
        }
    }
    /// <summary>
    /// Class f�r ai, som inte riktigt beter sig som Player
    /// men s� gott som
    /// 
    /// Skillnaden �r Wants_card(Deck) metoden
    /// </summary>
    public class Ai : Player
    {
        /// <summary>
        /// Exakt samma som Player konstrukt�ren
        /// Minns inte riktigt varf�r jag skapade denna
        /// </summary>
        /// <param name="_name">
        /// Se Player konstrukt�ren
        /// </param>
        public Ai(string _name) : base(_name)
        {
            name = _name;
        }
        /// <summary>
        /// En override av Players Wants_card(Deck) metod
        /// Detta f�renklar processen f�r mig
        /// D� b�da metodernas syfte �r det samma
        /// men dem l�ser det p� tv� olika s�tt
        /// 
        /// F�rst kollar metoden om Ai har mindre �n 17 po�ng
        /// Om den har det tar den ett kort
        /// Om den inte har det, men har mer �n 21 po�ng
        /// har ai f�rlorat och spelet �r �ver
        /// Om inget av det tidigare st�mmer
        /// vill ai inte ha n�gra fler kort
        /// </summary>
        /// <param name="deck">
        /// Se Wants_card Player
        /// </param>
        public override void Wants_card(Deck deck)
        {
            if (this.Sum() < 17)
            {
                this.Take_card(deck);
            }
            else if (this.Sum() > 21)
            {
                bust = true;
            }
            else
            {
                this.wants = false;
            }
        }
    }
}




