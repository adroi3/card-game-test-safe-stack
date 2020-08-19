namespace Game

open Shared

type Player =
    {
        Hand: PlayingCard list
        Name: string
        AwaitingCommands: AwaitingCommand list
        Order: int
    }