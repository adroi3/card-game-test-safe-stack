module Client

open Elmish
open Elmish.React
open Fable.React
open Fable.React.Props
open Fetch.Types
open Thoth.Fetch
open Fulma
open Shared

open CustomComponents

// The model holds data that you want to keep track of while the application is running
// in this case, we are keeping track of a counter
// we mark it as optional, because initially it will not be available from the client
// the initial value will be requested from server
type Model = { Counter: Counter option; Player: Player option }

// The Msg type defines what events/actions can occur while the application is running
// the state of the application changes *only* in reaction to these events
type Msg =
    | Increment
    | Decrement
    | PlayerLoaded of Player

let joinGame () =
    let properties = [ Method HttpMethod.PUT ]
    let url = Api.JoinGame + "?playerName=Dave"
    Fetch.fetchAs<_, Player> (url, properties = properties)

// defines the initial state and initial command (= side-effect) of the application
let init () : Model * Cmd<Msg> =
    let initialModel = { Counter = None; Player = None }
    let loadCommand =
        Cmd.OfPromise.perform joinGame () PlayerLoaded
    initialModel, loadCommand

// The update function computes the next state of the application based on the current state and the incoming events/messages
// It can also run side-effects (encoded as commands) like calling the server via Http.
// these commands in turn, can dispatch messages to which the update function will react.
let update (msg : Msg) (currentModel : Model) : Model * Cmd<Msg> =
    match currentModel.Counter, msg with
    | Some counter, Increment ->
        let nextModel = { currentModel with Counter = Some { Value = counter.Value + 5 } }
        nextModel, Cmd.none
    | Some counter, Decrement ->
        let nextModel = { currentModel with Counter = Some { Value = counter.Value - 5 } }
        nextModel, Cmd.none
    | _, PlayerLoaded playerLoaded->
        let nextModel = { Counter = Some { Value = 0 }; Player = Some playerLoaded }
        nextModel, Cmd.none
    | _ -> currentModel, Cmd.none

let show = function
    | { Counter = Some counter } -> string counter.Value
    | { Counter = None } -> "Loading..."

let button txt onClick =
    Button.button
        [ Button.IsFullWidth
          Button.Color IsPrimary
          Button.OnClick onClick ]
        [ str txt ]

let getCardColorNameString(playingCardColor: Color) =
    match playingCardColor with
    | Color.Hearts -> "heart"
    | Color.Tiles -> "diamond"
    | Color.Clovers -> "club"
    | Color.Pikes -> "spade"

let getCardFileName playingCard enemy =
    if enemy then
        "/cards/back.png"
    else
        "/cards/" + playingCard.CardType.ToString() + "_" + getCardColorNameString(playingCard.Color) + ".png"


let view (model : Model) (dispatch : Msg -> unit) =
    let showPlayingCards () =
            match model.Player with
            | Some player ->
                        player.Hand
                            |> List.map(fun playingCard ->
                                            img [   Class "mr-3 mt-1"
                                                    Style [ Width "10%" ; Height "200px" ]
                                                    Src (getCardFileName playingCard false)
                                                    Placeholder "image" ]
                                            )
             | _ -> [div [][]]

    div
        []
        [
            ofType<CustomComponents.JoinGameComponent,_,_> { data = 53 } []
            Heading.h3 [] [str "Spielernamen eingeben!"]
            Columns.columns []
                [
                   Column.column [] [ str (show model) ]
                   Column.column [] [ input [Type "text"] ]
                   Column.column [] [ button "Increment Counter" (fun _ -> dispatch Increment) ]
                ]
            div []
                (showPlayingCards())
        ]

#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif

Program.mkProgram init update view
#if DEBUG
|> Program.withConsoleTrace
#endif
|> Program.withReactBatched "elmish-app"
#if DEBUG
|> Program.withDebugger
#endif
|> Program.run
