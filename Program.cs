using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Data;
using Blackjack.Comms;


namespace Blackjack
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //Settings settings = new Settings();
            Console.Write("Hello World");
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Communication_server server = new();
            Communication_client com = new();
            while (true)
            {

            }
//Application.Run(new Start_window());
//Application.Run(new Game_window(settings));


        }
    }

    public class Game
    {
        readonly Player[] players;
        readonly Deck deck;

        public Game(int amount_player, int decks)
        {
            players = this.Player_creater(amount_player);
            deck = new Deck(decks);
        }

        private Player[] Player_creater(int amount)
        {
            Player[] temp = new Player[amount + 1];

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

            foreach(Player i in players)
            {
                if(i.Sum() > winner_sum && i.Sum() < 22)
                {
                    winner_sum = i.Sum();
                    winner = i.name;
                }
            }

            return (winner);
        }

        private void Start()
        {
            for (int j = 0; j < 2; j++)
            {
                foreach (Player i in players)
                {
                    i.Take_card(deck);
                }
            }
        }

        private void Mid()
        {
            bool loop = true;
            while (loop)
            {
                loop = false;
                foreach(Player i in players)
                {
                    if (i.wants)
                    {
                        i.Wants_card(deck);
                        loop = true; 
                    }
                }
            }
        }

        private void End()
        {
            //Print winner name on screen using Check_winner()
        }

        public void Loop()
        {
            Start();
            Mid();
            End();
        }

    }

    public class Card
    {
        public string color;
        public int value;
        public int realValue;

        public Card(string _color, int _value, int _realValue)
        {
            color = _color;
            value = _value;
            realValue = _realValue;
        }

        public void SwitchAce()
        {
            if (realValue == 11)
            {
                realValue = 1;
            }
        }
    }

    public class Deck
    {
        public List<Card> deck = new();

        public Deck(int amount)
        {
            deck = DeckCreater(amount);
        }

        public static List<Card> DeckCreater(int amount)
        {
            var temp_deck = new List<Card>();
            string[] colors = new string[] {"Heart", "Club", "Diamond", "Spade"};

            //Appends all cards to array
            for (int h = 0; h < amount; h++)
            foreach (string i in colors) {
                temp_deck.Add(new Card(i, 1, 11));
                for (int j = 2; j < 13; j++)
                {
                    temp_deck.Add(new Card(i, j, j));
                    if (j > 10)
                    {
                        temp_deck.Add(new Card(i, j, 10));
                    }
                }
            }
            //Return array of all cards
            return(temp_deck);
        }

        public Card GiveCard()
        {
            Card temp = deck[0];
            deck.RemoveAt(0);

            return(temp);
        }
    }

    public class Player
    {
        public List<Card> hand = new List<Card>();
        public string name;
        public bool wants = true;

        public Player(string _name)
        {
            name = _name;
        }

        public void Take_card(Deck deck)
        {
            //Adds card to hand
            hand.Add(deck.GiveCard());
        }

        public void Wants_card(Deck deck)
        {/*
            if () //Input from button
            {
                this.Take_card(deck);
            } else if ()
            {
                this.wants = false;
            }*/
        }

        private void Check_ace()
        {
            if (this.Sum() > 22)
            {
                foreach(Card i in hand)
                {
                    if (i.realValue == 11)
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
            foreach(Card i in hand)
            {
                output += i.realValue;
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

        public new void Wants_card(Deck deck)
        {
            if(this.Sum() < 17)
            {
                this.Take_card(deck);
            } else
            {
                this.wants = false;
            }
        }
    }

}
