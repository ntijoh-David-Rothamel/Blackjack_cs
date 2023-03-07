using System;
using System.Collections.Generic;

namespace test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Game game = new(1, 2);
        }
    }

    public class Game
    {
        readonly Player[] players;
        readonly Deck deck;
        public ConsoleKeyInfo keyInfo;

        public Game(int amount_player, int decks)
        {
            players = this.Player_creater(amount_player);
            deck = new Deck(decks);
            this.Loop();
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
            while (loop)
            {
                loop = false;
                this.Print_hands();
                foreach (Player i in this.players)
                {
                    if (i.wants)
                    {
                        i.Wants_card(this.deck, keyInfo);
                        loop = true;
                    }
                }
            }
        }

        private void End()
        {
            //Print winner name on screen using Check_winner()
            Console.Write(this.Check_winner());
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
        public int realValue;

        public Card(string _color, int _value, int _realValue)
        {
            color = _color;
            value = _value;
            realValue = _realValue;
        }

        public void SwitchAce()
        {
            if (this.value == 1)
            {
                this.realValue = (this.realValue == 1) ? 11 : 1;
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

        public Player(string _name)
        {
            name = _name;
        }

        public void Print_hand()
        {
            Console.WriteLine(this.name);
            foreach (Card i in this.hand)
            {
                Console.WriteLine(i.value);
            }
            Console.WriteLine(this.Sum());
            Console.WriteLine("");
        }

        public void Take_card(Deck deck)
        {
            //Adds card to hand
            this.hand.Add(deck.GiveCard());
            this.Check_ace();
        }

        public virtual void Wants_card(Deck deck, ConsoleKeyInfo keyInfo)
        {
            Console.WriteLine("");
            Console.WriteLine("{0}", this.name);
            Console.WriteLine("Do you want a card? (y/n)");

            keyInfo = Console.ReadKey();

            if (keyInfo.KeyChar == 'y') //Input from button
            {
                this.Take_card(deck);
            }
            else if (keyInfo.KeyChar == 'n')
            {
                this.wants = false;
            }
        }

        private void Check_ace()
        {
            if (this.Sum() > 22)
            {
                foreach (Card i in this.hand)
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
            foreach (Card i in hand)
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

        public override void Wants_card(Deck deck, ConsoleKeyInfo notUsed)
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
