namespace Ns1

[<RequireQualifiedAccess>]
module Module =
    module Nested =
        type U =
            | A of int

namespace Ns2

module Module =
    let a{caret} = Ns1.Module.Nested.A 1
