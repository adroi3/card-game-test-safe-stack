namespace CustomComponents

open Fable.React
open Fable.Core.JS

type JoinGameComponentProps = {
    data : int
}

type JoinGameComponent(initialProps) =
    inherit Component<JoinGameComponentProps, obj>(initialProps)
    do
      base.setInitState({ data = initialProps.data })

    override this.render() =
        h1 [] [str ("Hello World " + this.props.data.ToString())]

    override this.componentDidMount() =
        // Do something useful here... ;)
        console.log("Component Did Mount!")