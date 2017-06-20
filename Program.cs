using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C_Sharp_BlackJack_Game_Final_Console
{
    // Black Jack Console Game - Final Project for Programming Foundations in C# 
    // Developer: Linda L Marsh
    // Date:  6/01/2017
    // Summary of Game:
    //    Goal of the game is to have a hand greater than the dealer but not greater than 21
    //    The value for numbered cards are their face value, the Jack, Queen, King is worth 10 
    //    While the user or player has the option to change the value of the Ace, from 1 or 11
    //    To a number between 1 and 10.  The dealer must stand on 17.
    //


    public enum GameResult   // Set indicators for Games results
    {
        Win = 1, Lose = -1, Draw = 0, Pending = 2
    };

    public class Card   // Define The Cards
    {
        public string ID { get; set; }
        public string Suit { get; set; }
        public int Value { get; set; }

        public Card(string id, string suit, int value)
        {
            ID = id;
            Suit = suit;
            Value = value;
        }
    }

    public class Deck : Stack<Card>  // Define and Stack the Deck of Cards
    {
        public Deck(IEnumerable<Card> collection) : base(collection) { }
        public Deck() : base(52) { }

        public Card this[int index]
        {
            get
            {
                Card item;
                if (index >= 0 && index <= this.Count - 1)
                {
                    item = this.ToArray()[index];
                }
                else
                {
                    item = null;
                }
                return item;
            }
        }
        public double Value  // Get the value of the deck
        {
            get { return BlackJackRules.HandValue(this); }
        }
    }

    public class Member     // Class to represent both the Player and the Dealer
    {
        public Deck Hand;

        public Member()
        {
            Hand = new Deck();
        }
    }

    public static class BlackJackRules
    {
        //  Define the rules for the game.  
        //  Set an array to contain the values (ids) and suits of the cards.
        public static string[] ids = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "A", "J", "K", "Q" };
        public static string[] suits = { "Club", "Diamond", "Heart", "Spade" };
        public static int AceValChgd = 0;
        public static int aceNewVal = 0;
        public static int hitting = 0;

        //  Return a new deck of cards, resetting the value of the Aces if the player
        //  has chosen to assign them a new value
        public static Deck NewDeck
        {
            get
            {
                Deck d = new Deck();
                int value;

                foreach (string suit in suits)
                {
                    foreach (string id in ids)
                    {
                        if (id == "A")
                        {
                            if (AceValChgd == 1)
                            {
                                value = aceNewVal;
                            }
                            else
                            {
                                value = 10;
                            }
                        }
                        else
                        {
                            value = Int32.TryParse(id, out value) ? value : id == "A" ? 1 : 10;
                        }
                        d.Push(new Card(id, suit, value));
                    }
                }
                return d;
            }
        }

        public static Deck ShuffledDeck     // Shuffle and return a deck of cards
        {
            get
            {
                return new Deck(NewDeck.OrderBy(card => System.Guid.NewGuid()).ToArray());
            }
        }

        // Calculate the value of a hand.  
        // Return the value for Aces that is closest to or less than or equal to 21 unless user changed value
        // If user changed the value of the ace calculate using the new value
        public static double HandValue(Deck deck)
        {
            int val1 = 0;
            double val2 = 0;
            double aces = 0;

            if (AceValChgd == 1)  //If the Player changed the value for the Ace
            {
                val1 = deck.Sum(c => c.Value); //Calculate the value of the cards
                val2 = 0; //No need to add 10 as Ace only has one value
            }
            else //Aces default to 1 or 11
            {
                aces = deck.Count(c => c.ID == "A");  //Count the number of Aces
                val1 = deck.Sum(c => c.Value); //calculate the value of the cards with Ace = 1

                val2 = (aces != 0) ? val1 + (10 * aces) : val1; //Add the extra value for any Aces }
            }
            return new double[] { val1, val2 }  // Return the hand value
                .Select(handVal => new
                { handVal, weight = Math.Abs(handVal - 21) + (handVal > 21 ? 100 : 0) })
                .First().handVal;
        }

        // Check current value of Dealers hand to determine if the Dealer can hit ( or take another card)
        // Dealer must stand on 17 and not hit on soft 17

        public static bool CanDealerHit(Deck deck, int standLimit)
        {
            return deck.Value < standLimit;
        }

        public static bool CanPlayerHit(Deck deck)  // Determine if the Player can hit (if under 21)
        {
            return deck.Value < 21;
        }

        public static GameResult GetResult(Member player, Member dealer)
        {
            GameResult res = GameResult.Win;

            double playerValue = HandValue(player.Hand);
            double dealerValue = HandValue(dealer.Hand);

            if (playerValue <= 21)  // Determine if the player is has won, lost or drawn
            {
                if (playerValue != dealerValue)
                {
                    double closestValue = new double[] { playerValue, dealerValue }
                        .Select(handVal => new
                        {
                            handVal,
                            weight = Math.Abs(handVal - 21) + (handVal > 21 ? 100 : 0)
                        })
                            .OrderBy(n => n.weight)
                            .First().handVal;

                    res = playerValue == closestValue ? GameResult.Win : GameResult.Lose;
                }
                else
                {
                    res = GameResult.Draw;
                }
            }
            else
            {
                if (playerValue > 21)
                {
                    res = GameResult.Lose;
                }
            }
            return res;
        }
    }

    public class BlackJack
    {
        public Member Dealer = new Member();
        public Member Player = new Member();
        public GameResult Result { get; set; }
        public Deck MainDeck;
        public int StandLimit { get; set; }
        public BlackJack(int dealerStandLimit)
        {
            //Begin a new game
            Result = GameResult.Pending;
            StandLimit = dealerStandLimit;
            MainDeck = BlackJackRules.ShuffledDeck;

            Dealer.Hand.Clear();
            Player.Hand.Clear();

            //Deal two cards to Player and Dealer
            for (int i = 0; ++i < 3;)
            {
                Dealer.Hand.Push(MainDeck.Pop());
                Player.Hand.Push(MainDeck.Pop());
            }
        }

        public void Hit()  // Allow the player to hit (take a card)
        {
            if (BlackJackRules.CanPlayerHit(Player.Hand) && Result == GameResult.Pending)
            {
                if (BlackJackRules.hitting != 0)
                {
                    Player.Hand.Push(MainDeck.Pop());
                }
                Result = BlackJackRules.GetResult(Player, Dealer);
            }
        }

        public void Stand()  // Dealers turn after the player stands
        {
            if (Result == GameResult.Pending)
            {
                while (BlackJackRules.CanDealerHit(Dealer.Hand, StandLimit))
                {
                    Dealer.Hand.Push(MainDeck.Pop());
                }
                Result = BlackJackRules.GetResult(Player, Dealer);
            }
        }
    }

    public class Program
    {

        public static void ShowStats(BlackJack bj)
        {
            Console.WriteLine("The Dealer Has drawn: ");
            foreach (Card c in bj.Dealer.Hand)
            {
                string showSuit = c.Suit;
                string showCard = c.ID;
                Console.WriteLine(showCard + " " + showSuit);
            }

            Console.WriteLine("for a total of: " + bj.Dealer.Hand.Value);
            Console.WriteLine();

            Console.WriteLine("You the Player have drawn: ");

            foreach (Card c in bj.Player.Hand)
            {
                string showSuit = c.Suit;
                string showCard = c.ID;
                Console.WriteLine(showCard + " " + showSuit);
            }

            Console.WriteLine("for a total of: " + bj.Player.Hand.Value);
            if (bj.Player.Hand.Value > 21) { bj.Result = GameResult.Lose; }
        }

        public static void Main()
        {
            // Announce the Game and ask if user (player) would like to change
            // the value of the Ace before we begin the deal.
            Console.WriteLine("WELCOME TO BLACKJACK");
            Console.WriteLine();
            Console.WriteLine("The current value of the Ace is 1 or 11");
            Console.WriteLine();
            Console.WriteLine("In this game you have the option to assign a value to all Aces.");
            Console.WriteLine("Press 'Y' if you wish to change the value for the Ace. Otherwise, press any key to continue...");

            string changeAce = "";
            changeAce = Console.ReadLine();

            if (changeAce.ToLower() == "y")
            {
                Console.WriteLine(" What value between 1 and 11 would you like to assign the Ace?");
                string newAce = Console.ReadLine();
                try
                {
                    BlackJackRules.AceValChgd = 1;
                    BlackJackRules.aceNewVal = Convert.ToInt32(newAce);
                }
                catch (FormatException)
                {
                    Console.WriteLine("The Ace can only be set to a digit.");
                }
                finally
                {
                    if (BlackJackRules.AceValChgd < 11)
                    {
                        Console.WriteLine("The new value of all Aces is " + newAce);
                        Console.WriteLine();
                    }
                    else
                    {
                        Console.WriteLine("The Ace cannot be greater than 11, thus the value of Ace will be set to 11");
                    }
                }
            }

            string response = "";

            BlackJack bj = new BlackJack(17);
            ShowStats(bj);


            while (bj.Result == GameResult.Pending)
            {
                Console.WriteLine(" Press 'h' if you Would like to Hit? ");
                response = Console.ReadLine();
                Console.WriteLine();
                if (response.ToLower() == "h")
                {
                    BlackJackRules.hitting = 1;
                    bj.Hit();
                    ShowStats(bj);
                }
                else
                {
                    BlackJackRules.hitting = 0;
                    bj.Stand();
                    ShowStats(bj);
                }
            }
            // Display the correct message with the result of the game

            if (bj.Result == GameResult.Win) { Console.Write($"CONGRATULATIONS, you {bj.Result} this one!"); }

            if (bj.Result == GameResult.Draw) { Console.WriteLine($"The game was a {bj.Result}!"); }

            if (bj.Result == GameResult.Lose) { Console.WriteLine($"Sorry, you {bj.Result} this one!"); }

            Console.ReadLine();
        }
    }
}

