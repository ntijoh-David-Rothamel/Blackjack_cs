using Blackjack.Comms;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Blackjack
{
    class Program
    {

        static void Main(string[] args)
        {
            Program.AllocConsole();
            Game game = new(1, 2);
            while (true)
            {

            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            AllocConsole();
        }
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();
    }

    public class Game
    {
        readonly Player[] players;
        readonly Deck deck;
        public ConsoleKeyInfo keyInfo;
        public Communication_server server;
        public Game(int amount_player, int decks)
        {
            server = new();
            while (!server.done) { }
            Console.WriteLine("continued");
            players = this.Player_creater(amount_player);
            deck = new Deck(decks);
            this.Loop();
        }

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

        private string Check_winner()
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

        private void Print_hands()
        {
            foreach (Player i in this.players)
            {
                i.Print_hand();
            }
        }

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

        private void Mid()
        {
            bool loop = true;
            int x = 0;
            while (loop && x < 5)
            {
                loop = false;
                this.Print_hands();
                //Add something that sends the hand of player and ai
                foreach (Player i in this.players)
                {
                    i.Wants_card(this.deck);
                    if (i.wants)
                    {
                        loop = true;
                    }
                }
                x++;
            }
            Console.WriteLine("loop done");
        }

        private void End()
        {
            //Print winner name on screen using Check_winner()
            Console.Write(this.Check_winner());
            server.SendWinner(this.Check_winner());
        }

        private void Loop()
        {
            this.Start();
            this.Mid();
            this.End();
        }

    }

    public class Card
    {
        public string color;
        public int value;
        public int blackjackValue;

        public Card(string _color, int _value, int _blackjackValue)
        {
            color = _color;
            value = _value;
            blackjackValue = _blackjackValue;
        }

        public void SwitchAce()
        {
            if (this.value == 1)
            {
                this.blackjackValue = (this.blackjackValue == 1) ? 11 : 1;
            }
        }


    }

    public class Deck
    {
        public List<Card> deck = new();
        private Random rand = new();

        public Deck(int amount)
        {
            this.deck = DeckCreater(amount);
            this.Shuffle();
        }

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

        public Card GiveCard()
        {
            Card temp = deck[0];
            deck.RemoveAt(0);

            return (temp);
        }
    }

    public class Player
    {
        public List<Card> hand = new List<Card>();
        public string name;
        public bool wants = true;
        public static Communication_server server;

        public Player(string _name)
        {
            name = _name;
        }

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

        public void Take_card(Deck deck)
        {
            //Adds card to hand
            this.hand.Add(deck.GiveCard());
            this.Check_ace();
        }
        //Rewrite so this calls on server to ask client for input, 
        //Client gives server response
        //Server gives response to this
        public virtual void Wants_card(Deck deck)
        {
            if (this.Sum() < 21)
            {
                String confirmation = null;
                while (confirmation != "y" && confirmation != "n")
                {
                    confirmation = Player.server.AskForInput();
                }
                //Problem is here
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
        }


        private void Check_ace()
        {
            if (this.Sum() > 22)
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

    public class Ai : Player
    {
        public Ai(string _name) : base(_name)
        {
            name = _name;
        }

        public override void Wants_card(Deck deck)
        {
            if (this.Sum() < 17)
            {
                this.Take_card(deck);
            }
            else
            {
                this.wants = false;
            }
        }
    }
}




