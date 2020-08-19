open System.IO
open System.Threading.Tasks

open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open FSharp.Control.Tasks.V2
open Giraffe
open Saturn
open Shared
open Game


let tryGetEnv key =
    match Environment.GetEnvironmentVariable key with
    | x when String.IsNullOrWhiteSpace x -> None
    | x -> Some x

let publicPath = Path.GetFullPath "../Client/public"

let port =
    "SERVER_PORT"
    |> tryGetEnv |> Option.map uint16 |> Option.defaultValue 8085us

let webApp = router {
    get "/api/init" (fun next ctx ->
        task {
            let counter = {Value = 42}
            return! json counter next ctx
        })
    get Api.AmountOfRemainingCards (fun next ctx ->
        task {
            let amountOfRemainingCards = GameManager.getAmountOfRemainingCards()
            return! json amountOfRemainingCards next ctx
        })
    put Api.JoinGame (fun next ctx ->
        task {
            let player = match ctx.GetQueryStringValue "playerName" with
                         | Error msg ->
                            None
                         | Ok playerName ->
                            Some (GameManager.joinGame playerName)

           return! json player next ctx
        })
    post Api.DrawACard (fun next ctx ->
        task {
           let drawnCard = match ctx.GetQueryStringValue "playerName" with
                           | Error msg ->
                               None
                           | Ok playerName ->
                               Some (GameManager.drawACard playerName)

           return! json drawnCard next ctx
        })
    post Api.PlayACard (fun next ctx ->
        task {
           let playerName = match ctx.GetQueryStringValue "playerName" with
                            | Error msg ->
                                None
                            | Ok playerName ->
                                Some playerName

           let cardNumber = match ctx.GetQueryStringValue "cardNumber" with
                             | Error msg ->
                                None
                             | Ok cardNumber ->
                                Some (int cardNumber)

           let playedCard = if playerName <> None || cardNumber <> None then
                                Some (GameManager.playACard playerName.Value cardNumber.Value)
                            else
                                None

           return! json playedCard next ctx
        })
}

let app = application {
    url ("http://0.0.0.0:" + port.ToString() + "/")
    use_router webApp
    memory_cache
    use_static publicPath
    use_json_serializer(Thoth.Json.Giraffe.ThothSerializer())
    use_gzip
}

run app
