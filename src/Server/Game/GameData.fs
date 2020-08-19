namespace Game

open Shared

module GameData =
    [<Literal>]
    let AmountOfCardsPerColor = 13

    [<Literal>]
    let AmountOfCards = 52

    [<Literal>]
    let AmountOfHandCards = 7

    let mutable Deck: PlayingCard list = []
    let mutable Players: Game.Player list = []
    let mutable DiscardPile: PlayingCard list = []

    type HandCommand =
        | Draw of PlayingCard
        | Discard of int