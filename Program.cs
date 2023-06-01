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
        //Metod för consolen
        private void Form1_Load(object sender, EventArgs e)
        {
            AllocConsole();
        }
        //Gör något med consolen
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();
    }

    /// <summary>
    /// Själva spelet
    /// </summary>
    public class Game
    {
        //Array för att hålla alla spelare
        readonly Player[] players;
        //Kortleken
        readonly Deck deck;
        //Servern
        public Communication_server server;
        //Konstruktörmetoden, kallas när objectet instantieras
        //Metoden skapar servern och väntar tills den är färdig
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
        //Även Ai
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
        /// Avgör vem som har vunnit i slutet genom att loopa genom hela arrayen med spelare
        /// Kollar på summan av varje spelares hand med hjälp av sum() metoden
        /// </summary>
        /// <returns>
        /// Sträng, Tillbaka ger den en sträng som är namnet på vinnaren (Ai/Player 1)
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
        /// Kallar på print_hand() metoden hos varje spelare.
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
        /// Kallar på Take_card(Déck) metoden hos varje spelare
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
        /// Själva spelet
        /// Består av en loop som bryts när ingen av spelarna vill ha ett kort
        /// Eller när någon av spelarna har gått över 21 poäng
        /// Printar först ut händerna med hjälp av Print_hands()
        /// Sedan loopar den genom varje spelar och frågar om hen vill ha ett kort
        /// Med hjälp av Wants_card(Deck) metoden
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
        /// Kollar med hjälp av metoden Check_winner() vem som har vunnit
        /// Skickar det sedan till clienten
        /// </summary>
        private void End()
        {
            //Print winner name on screen using Check_winner()
            Console.Write(this.Check_winner());
            server.SendWinner(this.Check_winner());
        }
        /// <summary>
        /// Metod för att kalla på de olika stegen i spelet
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
    /// Själva kortet
    /// </summary>
    public class Card
    {
        public string color;
        public int value;
        public int blackjackValue;
        /// <summary>
        /// Ger kortet dess olika värden
        /// </summary>
        /// <param name="_color">Dess "färg" Deck klassen kommer ge den Club, Heart, Diamond or Spade, som sträng</param> 
        /// <param name="_value">Dess vanliga värde. Alltså ess till kung, som int</param>
        /// <param name="_blackjackValue">Dess värde i blackjack. Ess börjar med värdet 11, 10 till kung är värda 10 resterande kort har value blackjackvalue</param>
        public Card(string _color, int _value, int _blackjackValue)
        {
            color = _color;
            value = _value;
            blackjackValue = _blackjackValue;
        }
        /// <summary>
        /// Metod för att byta värdet på ess, det den gör är att den kollar att det är ett ess
        /// Och sedan byter den dess blackjackvalue till 1 om den var 11 eller tvärtom
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
    /// Förvarar alla korten i en lista
    /// </summary>
    public class Deck
    {
        public List<Card> deck = new();
        private Random rand = new();
        /// <summary>
        /// Kallar på metoden för att fylla kortleken med kort
        /// och sedan metoden för att blanda den
        /// </summary>
        /// <param name="amount">
        /// Int, avgör hur många kortlekar det ska vara
        /// </param>
        public Deck(int amount)
        {
            this.deck = DeckCreater(amount);
            this.Shuffle();
        }

        /// <summary>
        /// Instantierar kort och appendar dem sedan till en temporär lista,
        /// som håller korten i metoden.
        /// Den gör detta genom att loopa genom varje steg i arrayen colors
        /// Arrayen avgör sedan vilken färg det ska vara på korten som skapas
        /// Därefter loopar den tretton varv och ger kortet varvnumret till kortets value och blackjackvalue
        /// Undantaget är för knekt och uppåt som istället får blackjackvalue 10
        /// Ess instantieras separat med dess specifika värden
        /// </summary>
        /// <param name="amount">
        /// Int, Avgör hur många varv yttersta loopen ska gå
        /// och därmed hur många kortleks mängder av kort som ska instantieras
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
        /// Blandar runt korten genom att byta plats på två kort
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
        /// Sparar kortet temporärt
        /// och tar bort det från deck listan
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
    /// Har en lista för att hålla i korten
    /// och en server variabel för att kunna kalla på metoder hos servern
    /// Det är denna klass som spelaren interagerar genom
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
        /// Sträng, Ska helst vara ett namn
        /// </param>
        public Player(string _name)
        {
            name = _name;
        }
        /// <summary>
        /// Används för att printa ut spelarens hand till clienten
        /// Användes servens SendHand(Sträng) metod
        /// Som argument passerar den Hand_to_code() metoden
        /// Som omvandlar spelarens hand till en sträng som clienten kan tolka.
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
        /// Loopar genom alla kort i spelarens hand och omvandlar varje färg till en bokstav
        /// omvandlar också dess värde till en siffra eller bokstav
        /// Använder en switch case för att enkelt checka av alla de möjliga varianterna
        /// I slutet på strängen hamnar också summan av spelarens hand
        /// </summary>
        /// <returns>
        /// Sträng, Returnerar en sträng som servern enkelt kan skicka iväg till clienten
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
        /// Tar kort från kortleken och appendar det till spelarens lista
        /// som symboliserar spelarens hand
        /// Kollar även om essen behöver byta värde
        /// </summary>
        /// <param name="deck">
        /// Argumentet är Deck alltså en referens till kortleken
        /// så att den vet vad den ska interagera med
        /// </param>
        public void Take_card(Deck deck)
        {
            //Adds card to hand
            this.hand.Add(deck.GiveCard());
            this.Check_ace();
        }

        /// <summary>
        /// Kollar om spelaren vill ha ett kort
        /// Första kravet är att spelaren har mindre än 21 poäng
        /// Om hen inte har det förlorar spelaren
        /// Annars frågar den spelaren om hen vill ha ett kort via servern
        /// genom AskForInput() metoden
        /// Den väntar på den metoden till svaret är y || n
        /// Om svaret blir y kallar den på Take_card(Deck) metoden
        /// Annars sätter den bool variabeln wants till false
        /// Detta leder till att spelaren inte frågas igen
        /// </summary>
        /// <param name="deck">
        /// Argumentet är kortleken så att det sedan kan användas som argument i Take_card(Deck)
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
        /// Kollar om esset behöver byta värde
        /// Kravet är att spelaren har mer än 21 poäng
        /// Sedan letar den upp första esset som är värt 11 i spelarhanden
        /// och använder dess SwitchAce() metod
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
        /// Räknar ut värdet på spelarhanden
        /// Den gör det genom att iterera genom alla kort i spelarlistan
        /// och sedan lägger till dess blackjackvalue till en summa variabel
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
    /// Class för ai, som inte riktigt beter sig som Player
    /// men så gott som
    /// 
    /// Skillnaden är Wants_card(Deck) metoden
    /// </summary>
    public class Ai : Player
    {
        /// <summary>
        /// Exakt samma som Player konstruktören
        /// Minns inte riktigt varför jag skapade denna
        /// </summary>
        /// <param name="_name">
        /// Se Player konstruktören
        /// </param>
        public Ai(string _name) : base(_name)
        {
            name = _name;
        }
        /// <summary>
        /// En override av Players Wants_card(Deck) metod
        /// Detta förenklar processen för mig
        /// Då båda metodernas syfte är det samma
        /// men dem löser det på två olika sätt
        /// 
        /// Först kollar metoden om Ai har mindre än 17 poäng
        /// Om den har det tar den ett kort
        /// Om den inte har det, men har mer än 21 poäng
        /// har ai förlorat och spelet är över
        /// Om inget av det tidigare stämmer
        /// vill ai inte ha några fler kort
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




