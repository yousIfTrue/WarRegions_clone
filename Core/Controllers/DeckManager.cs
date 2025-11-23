using System;
using System.Collections.Generic;
using System.Linq;

namespace WarRegions.Core.Controllers
{
    // Core/Controllers/DeckManager.cs
    // Dependencies:
    // - Models/Units/UnitDeck.cs
    // - Models/Units/UnitInventory.cs
    // - Models/Player.cs
    // - Models/Units/UnitCard.cs
    
    using System;
    using System.Collections.Generic;
    using System.Linq;
    
    namespace WarRegionsClone.Controllers
    {
        public class DeckManager
        {
            private List<UnitDeck> _templateDecks;
            
            public DeckManager()
            {
                _templateDecks = new List<UnitDeck>();
                CreateTemplateDecks();
                Console.WriteLine("[DECK] DeckManager initialized");
            }
            
            private void CreateTemplateDecks()
            {
                // Balanced deck
                var balancedDeck = new UnitDeck("Balanced Army")
                {
                    MaxDeckSize = 6,
                    MaxSameUnitType = 2,
                    MaxRarityCount = 2
                };
                _templateDecks.Add(balancedDeck);
                
                // Aggressive deck
                var aggressiveDeck = new UnitDeck("Aggressive Strike")
                {
                    MaxDeckSize = 6,
                    MaxSameUnitType = 3,
                    MaxRarityCount = 1
                };
                _templateDecks.Add(aggressiveDeck);
                
                // Defensive deck
                var defensiveDeck = new UnitDeck("Defensive Formation")
                {
                    MaxDeckSize = 6,
                    MaxSameUnitType = 2,
                    MaxRarityCount = 1
                };
                _templateDecks.Add(defensiveDeck);
                
                // Fast attack deck
                var fastDeck = new UnitDeck("Swift Assault")
                {
                    MaxDeckSize = 6,
                    MaxSameUnitType = 2,
                    MaxRarityCount = 2
                };
                _templateDecks.Add(fastDeck);
                
                Console.WriteLine($"[DECK] Created {_templateDecks.Count} template decks");
            }
            
            public bool CreateDeckForPlayer(Player player, string deckName, List<UnitCard> units = null)
            {
                if (player.UnitInventory == null)
                {
                    Console.WriteLine("[DECK] Player has no unit inventory");
                    return false;
                }
                
                if (player.UnitInventory.Decks.Count >= player.UnitInventory.MaxDeckCapacity)
                {
                    Console.WriteLine($"[DECK] Player has reached maximum deck capacity ({player.UnitInventory.MaxDeckCapacity})");
                    return false;
                }
                
                var newDeck = new UnitDeck(deckName);
                
                // Add units if provided
                if (units != null)
                {
                    foreach (var unit in units)
                    {
                        if (!newDeck.AddUnit(unit))
                        {
                            Console.WriteLine($"[DECK] Failed to add {unit.UnitName} to new deck");
                        }
                    }
                }
                
                player.UnitInventory.Decks.Add(newDeck);
                Console.WriteLine($"[DECK] Created new deck '{deckName}' for {player.PlayerName}");
                
                return true;
            }
            
            public bool DeletePlayerDeck(Player player, string deckName)
            {
                if (player.UnitInventory == null)
                {
                    Console.WriteLine("[DECK] Player has no unit inventory");
                    return false;
                }
                
                var deckToRemove = player.UnitInventory.Decks.FirstOrDefault(d => d.DeckName.Equals(deckName, StringComparison.OrdinalIgnoreCase));
                if (deckToRemove == null)
                {
                    Console.WriteLine($"[DECK] Deck '{deckName}' not found");
                    return false;
                }
                
                if (deckToRemove == player.UnitInventory.ActiveDeck)
                {
                    Console.WriteLine($"[DECK] Cannot delete active deck '{deckName}'");
                    return false;
                }
                
                player.UnitInventory.Decks.Remove(deckToRemove);
                Console.WriteLine($"[DECK] Deleted deck '{deckName}' for {player.PlayerName}");
                
                return true;
            }
            
            public bool SetActiveDeck(Player player, string deckName)
            {
                if (player.UnitInventory == null)
                {
                    Console.WriteLine("[DECK] Player has no unit inventory");
                    return false;
                }
                
                var deck = player.UnitInventory.Decks.FirstOrDefault(d => d.DeckName.Equals(deckName, StringComparison.OrdinalIgnoreCase));
                if (deck == null)
                {
                    Console.WriteLine($"[DECK] Deck '{deckName}' not found");
                    return false;
                }
                
                if (!deck.IsValid())
                {
                    Console.WriteLine($"[DECK] Deck '{deckName}' is not valid");
                    return false;
                }
                
                player.UnitInventory.ActiveDeck = deck;
                Console.WriteLine($"[DECK] Set active deck to '{deckName}' for {player.PlayerName}");
                
                return true;
            }
            
            public bool AddUnitToDeck(Player player, string deckName, UnitCard unit)
            {
                if (player.UnitInventory == null)
                {
                    Console.WriteLine("[DECK] Player has no unit inventory");
                    return false;
                }
                
                var deck = player.UnitInventory.Decks.FirstOrDefault(d => d.DeckName.Equals(deckName, StringComparison.OrdinalIgnoreCase));
                if (deck == null)
                {
                    Console.WriteLine($"[DECK] Deck '{deckName}' not found");
                    return false;
                }
                
                if (!player.UnitInventory.AvailableUnits.Contains(unit))
                {
                    Console.WriteLine($"[DECK] Unit {unit.UnitName} not in player's inventory");
                    return false;
                }
                
                return deck.AddUnit(unit);
            }
            
            public bool RemoveUnitFromDeck(Player player, string deckName, UnitCard unit)
            {
                if (player.UnitInventory == null)
                {
                    Console.WriteLine("[DECK] Player has no unit inventory");
                    return false;
                }
                
                var deck = player.UnitInventory.Decks.FirstOrDefault(d => d.DeckName.Equals(deckName, StringComparison.OrdinalIgnoreCase));
                if (deck == null)
                {
                    Console.WriteLine($"[DECK] Deck '{deckName}' not found");
                    return false;
                }
                
                return deck.RemoveUnit(unit);
            }
            
            public UnitDeck GetDeckRecommendation(Player player, string strategy = "balanced")
            {
                var availableUnits = player.UnitInventory?.AvailableUnits ?? new List<UnitCard>();
                
                if (availableUnits.Count == 0)
                {
                    Console.WriteLine("[DECK] No units available for deck recommendation");
                    return null;
                }
                
                var recommendedDeck = new UnitDeck($"Recommended {strategy} Deck");
                
                // Filter and sort units based on strategy
                var filteredUnits = strategy.ToLower() switch
                {
                    "aggressive" => availableUnits
                        .OrderByDescending(u => u.Stats.Attack)
                        .ThenByDescending(u => u.Stats.Speed)
                        .Take(recommendedDeck.MaxDeckSize),
                        
                    "defensive" => availableUnits
                        .OrderByDescending(u => u.Stats.Defense)
                        .ThenByDescending(u => u.Stats.MaxHealth)
                        .Take(recommendedDeck.MaxDeckSize),
                        
                    "fast" => availableUnits
                        .OrderByDescending(u => u.Stats.Speed)
                        .ThenByDescending(u => u.Stats.Attack)
                        .Take(recommendedDeck.MaxDeckSize),
                        
                    "balanced" => availableUnits
                        .OrderByDescending(u => (u.Stats.Attack + u.Stats.Defense) / 2.0)
                        .ThenByDescending(u => u.Stats.MaxHealth)
                        .Take(recommendedDeck.MaxDeckSize),
                        
                    _ => availableUnits.Take(recommendedDeck.MaxDeckSize)
                };
                
                foreach (var unit in filteredUnits)
                {
                    recommendedDeck.AddUnit(unit);
                }
                
                Console.WriteLine($"[DECK] Generated {strategy} deck recommendation with {recommendedDeck.Units.Count} units");
                
                return recommendedDeck;
            }
            
            public bool ValidateDeck(UnitDeck deck)
            {
                if (deck == null)
                {
                    Console.WriteLine("[DECK] Deck is null");
                    return false;
                }
                
                bool isValid = deck.IsValid();
                
                if (!isValid)
                {
                    Console.WriteLine($"[DECK] Deck '{deck.DeckName}' validation failed");
                }
                else
                {
                    Console.WriteLine($"[DECK] Deck '{deck.DeckName}' is valid");
                }
                
                return isValid;
            }
            
            public string GetDeckAnalysis(UnitDeck deck)
            {
                if (deck == null || deck.Units.Count == 0)
                {
                    return "Deck is empty or null";
                }
                
                var analysis = new System.Text.StringBuilder();
                analysis.AppendLine($"Deck Analysis: {deck.DeckName}");
                analysis.AppendLine($"Total Units: {deck.Units.Count}/{deck.MaxDeckSize}");
                analysis.AppendLine($"Total Attack: {deck.TotalAttack}");
                analysis.AppendLine($"Total Defense: {deck.TotalDefense}");
                analysis.AppendLine($"Total Health: {deck.TotalHealth}");
                analysis.AppendLine($"Average Speed: {deck.AverageSpeed:F1}");
                analysis.AppendLine($"Total Cost: {deck.TotalSilverCost} silver, {deck.TotalGoldCost} gold");
                
                // Unit type distribution
                var typeDistribution = deck.Units
                    .GroupBy(u => u.UnitName.Split(' ').Last())
                    .ToDictionary(g => g.Key, g => g.Count());
                
                analysis.AppendLine("Unit Distribution:");
                foreach (var (unitType, count) in typeDistribution)
                {
                    analysis.AppendLine($"  {unitType}: {count}/{deck.MaxSameUnitType}");
                }
                
                // Rarity distribution
                var rarityDistribution = deck.Units
                    .GroupBy(u => u.Rarity)
                    .ToDictionary(g => g.Key, g => g.Count());
                
                analysis.AppendLine("Rarity Distribution:");
                foreach (var (rarity, count) in rarityDistribution)
                {
                    analysis.AppendLine($"  {rarity}: {count}");
                }
                
                // Deck rating
                double rating = CalculateDeckRating(deck);
                analysis.AppendLine($"Deck Rating: {rating:F1}/10");
                analysis.AppendLine($"Recommendation: {GetDeckRecommendation(rating)}");
                
                return analysis.ToString();
            }
            
            private double CalculateDeckRating(UnitDeck deck)
            {
                if (deck.Units.Count == 0) return 0;
                
                double rating = 0;
                
                // Base rating for deck size
                double sizeRating = (double)deck.Units.Count / deck.MaxDeckSize * 3;
                rating += sizeRating;
                
                // Stat balance rating
                double statBalance = (deck.TotalAttack + deck.TotalDefense + deck.TotalHealth) / 3.0;
                double statRating = Math.Min(statBalance / 100, 4);
                rating += statRating;
                
                // Speed rating
                double speedRating = Math.Min(deck.AverageSpeed / 50, 2);
                rating += speedRating;
                
                // Cost efficiency rating
                double costEfficiency = (deck.TotalAttack + deck.TotalDefense) / (double)Math.Max(1, deck.TotalSilverCost / 10);
                double costRating = Math.Min(costEfficiency, 1);
                rating += costRating;
                
                return Math.Min(rating, 10);
            }
            
            private string GetDeckRecommendation(double rating)
            {
                return rating switch
                {
                    >= 9 => "Excellent deck! Well balanced and powerful.",
                    >= 7 => "Good deck. Could use some minor improvements.",
                    >= 5 => "Average deck. Consider optimizing unit composition.",
                    >= 3 => "Weak deck. Needs significant improvements.",
                    _ => "Poor deck. Should be completely redesigned."
                };
            }
            
            public void OptimizeDeck(UnitDeck deck, Player player, string focus = "balanced")
            {
                if (deck == null || player.UnitInventory == null)
                {
                    Console.WriteLine("[DECK] Cannot optimize - deck or player inventory is null");
                    return;
                }
                
                var availableUnits = player.UnitInventory.AvailableUnits;
                var currentUnits = new List<UnitCard>(deck.Units);
                
                deck.ClearDeck();
                
                // Re-add units based on optimization focus
                var optimizedUnits = focus.ToLower() switch
                {
                    "attack" => availableUnits
                        .OrderByDescending(u => u.Stats.Attack)
                        .ThenByDescending(u => u.Stats.Speed),
                        
                    "defense" => availableUnits
                        .OrderByDescending(u => u.Stats.Defense)
                        .ThenByDescending(u => u.Stats.MaxHealth),
                        
                    "speed" => availableUnits
                        .OrderByDescending(u => u.Stats.Speed)
                        .ThenByDescending(u => u.Stats.Attack),
                        
                    "cost" => availableUnits
                        .OrderBy(u => u.Stats.SilverCost)
                        .ThenBy(u => u.Stats.GoldCost),
                        
                    _ => availableUnits
                        .OrderByDescending(u => (u.Stats.Attack + u.Stats.Defense + u.Stats.MaxHealth) / 3.0)
                };
                
                foreach (var unit in optimizedUnits)
                {
                    if (deck.Units.Count >= deck.MaxDeckSize) break;
                    
                    if (deck.CanAddUnit(unit))
                    {
                        deck.AddUnit(unit);
                    }
                }
                
                Console.WriteLine($"[DECK] Optimized deck '{deck.DeckName}' with {focus} focus");
            }
            
            public List<UnitDeck> GetTemplateDecks()
            {
                return new List<UnitDeck>(_templateDecks);
            }
            
            public bool ApplyTemplateDeck(Player player, string templateName)
            {
                var template = _templateDecks.FirstOrDefault(d => d.DeckName.Equals(templateName, StringComparison.OrdinalIgnoreCase));
                if (template == null)
                {
                    Console.WriteLine($"[DECK] Template deck '{templateName}' not found");
                    return false;
                }
                
                // Create a copy of the template deck for the player
                var newDeck = template.Clone();
                newDeck.DeckName = $"{templateName} Copy";
                
                player.UnitInventory.Decks.Add(newDeck);
                Console.WriteLine($"[DECK] Applied template deck '{templateName}' to {player.PlayerName}");
                
                return true;
            }
            
            public void SaveDeckTemplate(UnitDeck deck, string templateName)
            {
                var template = deck.Clone();
                template.DeckName = templateName;
                
                _templateDecks.Add(template);
                Console.WriteLine($"[DECK] Saved deck as template: {templateName}");
            }
        }
    }}
