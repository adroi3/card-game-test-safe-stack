namespace Game

open Shared


module GameManager =

    let private generateCard cardNumber =
        let cardNumberInsideTheColor = ((cardNumber - 1) % GameData.AmountOfCardsPerColor) + 1
        let colorNumber = (cardNumber - 1) / GameData.AmountOfCardsPerColor + 1

        {
            CardType = enum cardNumberInsideTheColor
            Color = enum colorNumber
        }

    let private getPlayer playerName =
        (GameData.Players |> List.filter (fun x -> x.Name = playerName)).Head

    let private random = System.Random()

    let private shuffel deck =

        let deckAsArray = deck |> List.toArray

        let swap (deckAsArray: _[]) x y =
            let tmp = deckAsArray.[x]
            deckAsArray.[x] <- deckAsArray.[y]
            deckAsArray.[y] <- tmp

        // shuffle an array (in-place)
        Array.iteri (fun i _ -> swap deckAsArray i (random.Next(i, Array.length deckAsArray))) deckAsArray

        Array.toList deckAsArray

    let private generateDeck() =
        [1..GameData.AmountOfCards]
         |> List.map generateCard
         |> shuffel

    let private changeHand playerName handCommand =
        let oldPlayer = getPlayer playerName

        let newHandInfo = match handCommand with
                          | GameData.HandCommand.Draw drawnCard ->
                            ((oldPlayer.Hand @ [drawnCard]), drawnCard)
                          | GameData.HandCommand.Discard cardNumber ->
                            let newHand = [1..oldPlayer.Hand.Length]
                                        |> List.filter (fun x -> x <> cardNumber)
                                        |> List.map (fun x -> oldPlayer.Hand.Item (x-1))
                            let discardedCard = ([1..oldPlayer.Hand.Length]
                                              |> List.filter (fun x -> x = cardNumber)
                                              |> List.map (fun x -> oldPlayer.Hand.Item (x-1))).Head
                            (newHand, discardedCard)

        let updatedPlayer = { oldPlayer with Hand = fst newHandInfo; }

        GameData.Players <- (GameData.Players |> List.filter (fun x -> x.Name <> playerName)) @ [updatedPlayer]

        (oldPlayer, snd newHandInfo)


    let drawACard playerName =
        let drawnCard = GameData.Deck.Head

        GameData.Deck <- GameData.Deck.Tail

        snd (changeHand playerName (GameData.HandCommand.Draw drawnCard))

    let playACard playerName cardNumber =
        let player, playedCard = changeHand playerName (GameData.HandCommand.Discard cardNumber)

        // TODO: Effekte einfügen!

        GameData.DiscardPile <- GameData.DiscardPile @ [playedCard]

        playedCard

    let joinGame playerName  =
        if GameData.Deck.Length = 0 then
            GameData.Deck <- generateDeck()

        GameData.Players <- GameData.Players @ [{ Hand = []; Name = playerName; AwaitingCommands = []; Order = GameData.Players.Length }]

        [1..GameData.AmountOfHandCards]
            |> List.iter (fun x -> drawACard playerName |> ignore)

        let player = getPlayer playerName

        { Hand = player.Hand; Name = player.Name; Order = player.Order }

    let getAmountOfRemainingCards() =
        GameData.Deck.Length