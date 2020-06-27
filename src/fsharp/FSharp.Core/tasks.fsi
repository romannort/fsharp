// TaskBuilder.fs - TPL task computation expressions for F#
//
// Copyright (c) Microsoft Corporation.  All Rights Reserved.  See License.txt in the project root for license information.

namespace Microsoft.FSharp.Core.CompilerServices

    open Microsoft.FSharp.Core
    open System.Runtime.CompilerServices

    /// A marker interface to give priority to different available overloads.
    [<Experimental("Experimental library feature, requires '--langversion:preview'")>]
    type IPriority3 = interface end

    /// A marker interface to give priority to different available overloads. Overloads using a
    /// parameter of this type will be preferred to overloads with IPriority3,
    /// all else being equal.
    [<Experimental("Experimental library feature, requires '--langversion:preview'")>]
    type IPriority2 = interface inherit IPriority3 end

    /// A marker interface to give priority to different available overloads. Overloads using a
    /// parameter of this type will be preferred to overloads with IPriority2 or IPriority3,
    /// all else being equal.
    [<Experimental("Experimental library feature, requires '--langversion:preview'")>]
    type IPriority1 = interface inherit IPriority2 end

    /// Defines the implementation of the MoveNext method for a struct state machine.
    [<Experimental("Experimental library feature, requires '--langversion:preview'")>]
    type MoveNextMethod<'Template> = delegate of byref<'Template> -> unit

    /// Defines the implementation of the SetMachineState method for a struct state machine.
    [<Experimental("Experimental library feature, requires '--langversion:preview'")>]
    type SetMachineStateMethod<'Template> = delegate of byref<'Template> * IAsyncStateMachine -> unit

    /// Defines the implementation of the code reun after the creation of a struct state machine.
    [<Experimental("Experimental library feature, requires '--langversion:preview'")>]
    type AfterMethod<'Template, 'Result> = delegate of byref<'Template> -> 'Result

    /// Contains compiler intrinsics related to the definition of state machines.
    [<Experimental("Experimental library feature, requires '--langversion:preview'")>]
    module StateMachineHelpers = 

        /// Statically determines whether resumable code is being used
        [<MethodImpl(MethodImplOptions.NoInlining)>]
        [<Experimental("Experimental library feature, requires '--langversion:preview'")>]
        val __useResumableStateMachines<'T> : bool 

        /// Indicates a resumption point within resumable code
        [<MethodImpl(MethodImplOptions.NoInlining)>]
        [<Experimental("Experimental library feature, requires '--langversion:preview'")>]
        val __resumableEntry: unit -> int option

        /// Indicates to jump to a resumption point within resumable code.
        /// If the 'pc' is statically known then this is a 'goto' into resumable code.  If the 'pc' is not statically
        /// known it must be a valid resumption point within this block of resumable code.
        [<MethodImpl(MethodImplOptions.NoInlining)>]
        [<Experimental("Experimental library feature, requires '--langversion:preview'")>]
        val __resumeAt : pc: int -> 'T

        /// Attempts to convert a computation description to a state machine with resumable code 
        [<MethodImpl(MethodImplOptions.NoInlining)>]
        [<Experimental("Experimental library feature, requires '--langversion:preview'")>]
        val __resumableStateMachine<'T> : _obj: 'T -> 'T

        /// Within a compiled state machine, indicates the given methods provide implementations of the
        /// IAsyncStateMachine functionality for a struct state machine.
        ///
        /// The template type guides the generation of a new struct type.  Any mention of the template in
        /// any of the code is rewritten to the new struct type.  'moveNext' and 'setMachineState' are
        /// used to implement the methods on the interface implemented by the struct type. The 'after'
        /// method is executed after the state machine has been created.
        [<MethodImpl(MethodImplOptions.NoInlining)>]
        [<Experimental("Experimental library feature, requires '--langversion:preview'")>]
        val __resumableStateMachineStruct<'Template, 'Result> : moveNext: MoveNextMethod<'Template> -> _setMachineState: SetMachineStateMethod<'Template> -> after: AfterMethod<'Template, 'Result> -> 'Result

#if !BUILDING_WITH_LKG && !BUILD_FROM_SOURCE
namespace Microsoft.FSharp.Control

    open System
    open System.Runtime.CompilerServices
    open System.Threading.Tasks
    open Microsoft.FSharp.Core
    open Microsoft.FSharp.Core.CompilerServices
    open Microsoft.FSharp.Core.CompilerServices.StateMachineHelpers
    open Microsoft.FSharp.Control
    open Microsoft.FSharp.Collections

    [<Struct; NoComparison; NoEquality>]
    [<Experimental("Experimental library feature, requires '--langversion:preview'")>]
    [<CompilerMessage("This construct  is for use by compiled F# code and should not be used directly", 1204, IsHidden=true)>]
    /// This is used by the compiler as a template for creating state machine structs
    type TaskStateMachine<'T> =

        /// Holds the final result of the state machine
        [<DefaultValue(false)>]
        val mutable Result : 'T

        /// When statically compiled, holds the continuation goto-label further execution of the state machine
        [<DefaultValue(false)>]
        val mutable ResumptionPoint : int

        /// When interpreted, holds the continuation for the further execution of the state machine
        [<DefaultValue(false)>]
        val mutable ResumptionFunc : TaskMachineFunc<'T>

        /// When interpreted, holds the awaiter used to suspend of the state machine
        [<DefaultValue(false)>]
        val mutable Awaiter : ICriticalNotifyCompletion

        [<DefaultValue(false)>]
        val mutable MethodBuilder : AsyncTaskMethodBuilder<'T>

        // For debugging
        member Address: nativeint

        interface IAsyncStateMachine

    and
        [<Experimental("Experimental library feature, requires '--langversion:preview'")>]
        [<CompilerMessage("This construct  is for use by compiled F# code and should not be used directly", 1204, IsHidden=true)>]
        /// Contains the `task` computation expression builder.
        TaskMachineFunc<'TOverall> = delegate of byref<TaskStateMachine<'TOverall>> -> bool

    [<Experimental("Experimental library feature, requires '--langversion:preview'")>]
    [<CompilerMessage("This construct  is for use by compiled F# code and should not be used directly", 1204, IsHidden=true)>]
    type TaskCode<'TOverall, 'T> = delegate of byref<TaskStateMachine<'TOverall>> -> bool 

    [<Class>]
    [<Experimental("Experimental library feature, requires '--langversion:preview'")>]
    type TaskBuilder =
    
        [<Experimental("Experimental library feature, requires '--langversion:preview'")>]
        member inline Combine: task1: TaskCode<'TOverall, unit> * task2: TaskCode<'TOverall, 'T> -> TaskCode<'TOverall, 'T>
    
        [<Experimental("Experimental library feature, requires '--langversion:preview'")>]
        member inline Delay: f: (unit -> TaskCode<'TOverall, 'T>) -> TaskCode<'TOverall, 'T>
    
        [<Experimental("Experimental library feature, requires '--langversion:preview'")>]
        member inline For: sequence: seq<'T> * body: ('T -> TaskCode<'TOverall, unit>) -> TaskCode<'TOverall, unit>
    
        [<Experimental("Experimental library feature, requires '--langversion:preview'")>]
        member inline Return: x: 'T -> TaskCode<'T, 'T>
    
        [<Experimental("Experimental library feature, requires '--langversion:preview'")>]
        member inline Run: code: TaskCode<'T, 'T> -> Task<'T>
    
        [<Experimental("Experimental library feature, requires '--langversion:preview'")>]
        member inline TryFinally: body: TaskCode<'TOverall, 'T> * fin: (unit -> unit) -> TaskCode<'TOverall, 'T>
    
        [<Experimental("Experimental library feature, requires '--langversion:preview'")>]
        member inline TryWith: body: TaskCode<'TOverall, 'T> * catch: (exn -> TaskCode<'TOverall, 'T>) -> TaskCode<'TOverall, 'T>
    
        [<Experimental("Experimental library feature, requires '--langversion:preview'")>]
        member inline Using: disp: 'Resource * body: ('Resource -> TaskCode<'TOverall, 'T>) -> TaskCode<'TOverall, 'T> when 'Resource :> IDisposable
    
        [<Experimental("Experimental library feature, requires '--langversion:preview'")>]
        member inline While: condition: (unit -> bool) * body: TaskCode<'TOverall, unit> -> TaskCode<'TOverall, unit>
    
        [<DefaultValue>]
        [<Experimental("Experimental library feature, requires '--langversion:preview'")>]
        member inline Zero: unit -> TaskCode<'TOverall, unit>

        [<Experimental("Experimental library feature, requires '--langversion:preview'")>]
        member inline ReturnFrom: task: Task<'T> -> TaskCode<'T, 'T>

        /// The entry point for the dynamic implementation of the corresponding operation. Do not use directly, only used when executing quotations that involve tasks or other reflective execution of F# code.
        [<Experimental("Experimental library feature, requires '--langversion:preview'")>]
        static member RunDynamic: code: TaskCode<'T, 'T> -> Task<'T>
    
        /// The entry point for the dynamic implementation of the corresponding operation. Do not use directly, only used when executing quotations that involve tasks or other reflective execution of F# code.
        [<Experimental("Experimental library feature, requires '--langversion:preview'")>]
        static member CombineDynamic: task1: TaskCode<'TOverall, unit> * task2: TaskCode<'TOverall, 'T> -> TaskCode<'TOverall, 'T>
    
        /// The entry point for the dynamic implementation of the corresponding operation. Do not use directly, only used when executing quotations that involve tasks or other reflective execution of F# code.
        [<Experimental("Experimental library feature, requires '--langversion:preview'")>]
        static member WhileDynamic: condition: (unit -> bool) * body: TaskCode<'TOverall, unit> -> TaskCode<'TOverall, unit>
    
        /// The entry point for the dynamic implementation of the corresponding operation. Do not use directly, only used when executing quotations that involve tasks or other reflective execution of F# code.
        [<Experimental("Experimental library feature, requires '--langversion:preview'")>]
        static member TryFinallyDynamic: body: TaskCode<'TOverall, 'T> * fin: (unit -> unit) -> TaskCode<'TOverall, 'T>
    
        /// The entry point for the dynamic implementation of the corresponding operation. Do not use directly, only used when executing quotations that involve tasks or other reflective execution of F# code.
        [<Experimental("Experimental library feature, requires '--langversion:preview'")>]
        static member TryWithDynamic: body: TaskCode<'TOverall, 'T> * catch: (exn -> TaskCode<'TOverall, 'T>) -> TaskCode<'TOverall, 'T>
    
        /// The entry point for the dynamic implementation of the corresponding operation. Do not use directly, only used when executing quotations that involve tasks or other reflective execution of F# code.
        [<Experimental("Experimental library feature, requires '--langversion:preview'")>]
        static member ReturnFromDynamic: task: Task<'T> -> TaskCode<'T, 'T>

    /// Contains the `task` computation expression builder.
    [<AutoOpen>]
    [<Experimental("Experimental library feature, requires '--langversion:preview'")>]
    module TaskBuilder = 

        /// Builds a task using computation expression syntax
        [<Experimental("Experimental library feature, requires '--langversion:preview'")>]
        val task : TaskBuilder

    /// Contains extension methods allowing the `task` computation expression builder
    /// binding to tasks in a way that is sensitive to the current scheduling context.
    /// This module is automatically opened.
    [<AutoOpen>]
    module ContextSensitiveTasks = 

        /// Provides evidence that various types can be used in bind and return constructs in task computation expressions
        [<Sealed; NoComparison; NoEquality>]
        [<Experimental("Experimental library feature, requires '--langversion:preview'")>]
        type TaskWitnesses =
                interface IPriority1
                interface IPriority2
                interface IPriority3

                /// Provides evidence that task-like types can be used in 'bind' in a task computation expression
                [<NoDynamicInvocation>]
                [<Experimental("Experimental library feature, requires '--langversion:preview'")>]
                static member inline CanBind< ^TaskLike, ^TResult1, 'TResult2, ^Awaiter, 'TOverall >
                        : priority: IPriority2 * taskLike: ^TaskLike * k: ( ^TResult1 -> TaskCode<'TOverall, 'TResult2>) -> TaskCode<'TOverall, 'TResult2>
                                                    when  ^TaskLike: (member GetAwaiter:  unit ->  ^Awaiter)
                                                    and ^Awaiter :> ICriticalNotifyCompletion
                                                    and ^Awaiter: (member get_IsCompleted:  unit -> bool)
                                                    and ^Awaiter: (member GetResult:  unit ->  ^TResult1) 

                /// Provides evidence that tasks can be used in 'bind' in a task computation expression
                [<Experimental("Experimental library feature, requires '--langversion:preview'")>]
                static member inline CanBind: priority: IPriority1 * task: Task<'TResult1> * k: ('TResult1 -> TaskCode<'TOverall, 'TResult2>) -> TaskCode<'TOverall, 'TResult2>

                /// Provides evidence that F# Async computations can be used in 'bind' in a task computation expression
                [<Experimental("Experimental library feature, requires '--langversion:preview'")>]
                static member inline CanBind: priority: IPriority1 * computation: Async<'TResult1> * k: ('TResult1 -> TaskCode<'TOverall, 'TResult2>) -> TaskCode<'TOverall, 'TResult2>

                /// Provides evidence that task-like types can be used in 'return' in a task workflow
                [<NoDynamicInvocation>]
                [<Experimental("Experimental library feature, requires '--langversion:preview'")>]
                static member inline CanReturnFrom< ^TaskLike, ^Awaiter, ^T> : priority: IPriority2 * taskLike: ^TaskLike -> TaskCode< ^T, ^T > 
                                                    when  ^TaskLike: (member GetAwaiter:  unit ->  ^Awaiter)
                                                    and ^Awaiter :> ICriticalNotifyCompletion
                                                    and ^Awaiter: (member get_IsCompleted: unit -> bool)
                                                    and ^Awaiter: (member GetResult: unit ->  ^T)

                /// Provides evidence that F# Async computations can be used in 'return' in a task computation expression
                [<Experimental("Experimental library feature, requires '--langversion:preview'")>]
                static member inline CanReturnFrom: IPriority1 * computation: Task<'T> -> TaskCode<'T, 'T>

                /// Provides evidence that F# Async computations can be used in 'return' in a task computation expression
                [<Experimental("Experimental library feature, requires '--langversion:preview'")>]
                static member inline CanReturnFrom: IPriority1 * computation: Async<'T> -> TaskCode<'T, 'T>

                /// The entry point for the dynamic implementation of the corresponding operation. Do not use directly, only used when executing quotations that involve tasks or other reflective execution of F# code.
                [<Experimental("Experimental library feature, requires '--langversion:preview'")>]
                static member inline CanBindDynamic< ^TaskLike, ^TResult1, 'TResult2, ^Awaiter, 'TOverall >
                        : priority: IPriority2 * taskLike: ^TaskLike * k: ( ^TResult1 -> TaskCode<'TOverall, 'TResult2>) -> TaskCode<'TOverall, 'TResult2>
                                                    when  ^TaskLike: (member GetAwaiter:  unit ->  ^Awaiter)
                                                    and ^Awaiter :> ICriticalNotifyCompletion
                                                    and ^Awaiter: (member get_IsCompleted:  unit -> bool)
                                                    and ^Awaiter: (member GetResult:  unit ->  ^TResult1) 

                /// The entry point for the dynamic implementation of the corresponding operation. Do not use directly, only used when executing quotations that involve tasks or other reflective execution of F# code.
                [<Experimental("Experimental library feature, requires '--langversion:preview'")>]
                static member inline CanBindDynamic: priority: IPriority1 * task: Task<'TResult1> * k: ('TResult1 -> TaskCode<'TOverall, 'TResult2>) -> TaskCode<'TOverall, 'TResult2>

                /// The entry point for the dynamic implementation of the corresponding operation. Do not use directly, only used when executing quotations that involve tasks or other reflective execution of F# code.
                [<Experimental("Experimental library feature, requires '--langversion:preview'")>]
                static member inline CanReturnFromDynamic< ^TaskLike, ^Awaiter, ^T> : priority: IPriority2 * taskLike: ^TaskLike -> TaskCode< ^T, ^T > 
                                                    when  ^TaskLike: (member GetAwaiter:  unit ->  ^Awaiter)
                                                    and ^Awaiter :> ICriticalNotifyCompletion
                                                    and ^Awaiter: (member get_IsCompleted: unit -> bool)
                                                    and ^Awaiter: (member GetResult: unit ->  ^T)

                /// The entry point for the dynamic implementation of the corresponding operation. Do not use directly, only used when executing quotations that involve tasks or other reflective execution of F# code.
                [<Experimental("Experimental library feature, requires '--langversion:preview'")>]
                static member CanReturnFromDynamic: IPriority1 * computation: Task<'T> -> TaskCode<'T, 'T>

        [<AutoOpen>]
        module TaskHelpers = 

            type TaskBuilder with 
                /// Provides the ability to bind to a variety of tasks, using context-sensitive semantics
                [<Experimental("Experimental library feature, requires '--langversion:preview'")>]
                member inline Bind< ^TaskLike, ^TResult1, 'TResult2, 'TOverall
                                        when (TaskWitnesses or  ^TaskLike): (static member CanBind: TaskWitnesses * ^TaskLike * (^TResult1 -> TaskCode<'TOverall, 'TResult2>) -> TaskCode<'TOverall, 'TResult2>)> 
                             : task: ^TaskLike * continuation: (^TResult1 -> TaskCode<'TOverall, 'TResult2>) -> TaskCode<'TOverall, 'TResult2>        

                /// Provides the ability to return results from a variety of tasks, using context-sensitive semantics
                [<Experimental("Experimental library feature, requires '--langversion:preview'")>]
                member inline ReturnFrom: a: ^TaskLike -> TaskCode< 'T, 'T >
                    when (TaskWitnesses or  ^TaskLike): (static member CanReturnFrom: TaskWitnesses * ^TaskLike -> TaskCode<'T, 'T>)


(*
    /// Contains extension methods allowing the `task` computation expression builder
    /// to bind to tasks in a way that doesn't reschedule work in the current scheduling context.
    /// This is not suitable when tasks must interact with user interface controls on the same
    /// thread as its caller.
    ///
    /// This module is automatically opened.
    module ContextInsensitiveTasks = 

        /// Provides evidence that various types can be used in bind and return constructs in task computation expressions
        [<Sealed; NoComparison; NoEquality>]
        type TaskWitnesses =
            interface IPriority1
            interface IPriority2
            interface IPriority3

            /// Provides evidence that task-like computations can be used in 'bind' in a task computation expression
            [<NoDynamicInvocation>]
            static member inline CanBind< ^TaskLike, ^TResult1, 'TResult2, ^Awaiter, 'TOverall > : priority: IPriority3 * taskLike: ^TaskLike * k: ( ^TResult1 -> TaskCode<'TOverall, 'TResult2>) -> TaskCode<'TOverall, 'TResult2>
                when  ^TaskLike: (member GetAwaiter:  unit ->  ^Awaiter)
                and ^Awaiter :> ICriticalNotifyCompletion
                and ^Awaiter: (member get_IsCompleted:  unit -> bool)
                and ^Awaiter: (member GetResult:  unit ->  ^TResult1)

            /// Provides evidence that task-like computations can be used in 'bind' in a task computation expression
            [<NoDynamicInvocation>]
            static member inline CanBind< ^TaskLike, ^TResult1, 'TResult2, ^Awaitable, ^Awaiter, 'TOverall > : priority: IPriority2 * taskLike: ^TaskLike * k: (^TResult1 -> TaskCode<'TOverall, 'TResult2>) -> TaskCode<'TOverall, 'TResult2>
                when  ^TaskLike: (member ConfigureAwait: bool ->  ^Awaitable)
                and ^Awaitable: (member GetAwaiter:  unit ->  ^Awaiter)
                and ^Awaiter :> ICriticalNotifyCompletion
                and ^Awaiter: (member get_IsCompleted: unit -> bool)
                and ^Awaiter: (member GetResult: unit ->  ^TResult1)

            /// Provides evidence that tasks can be used in 'bind' in a task computation expression
            [<NoDynamicInvocation>]
            static member inline CanBind: priority: IPriority1 * task: Task<'TResult1> * k: ('TResult1 -> TaskCode<'TOverall, 'TResult2>) -> TaskCode<'TOverall, 'TResult2>

            /// Provides evidence that F# async computations can be used in 'bind' in a task computation expression
            [<NoDynamicInvocation>]
            static member inline CanBind: priority: IPriority1 * computation: Async<'TResult1> * k: ('TResult1 -> TaskCode<'TOverall, 'TResult2>) -> TaskCode<'TOverall, 'TResult2>

            /// Provides evidence that types following the "awaitable" pattern can be used in 'return!' in a task computation expression
            [<NoDynamicInvocation>]
            static member inline CanReturnFrom< ^Awaitable, ^Awaiter, ^T> : IPriority2 * taskLike: ^Awaitable -> TaskStep< ^T, ^T>
                                                    when  ^Awaitable: (member GetAwaiter: unit ->  ^Awaiter)
                                                    and ^Awaiter :> ICriticalNotifyCompletion
                                                    and ^Awaiter: (member get_IsCompleted: unit -> bool)
                                                    and ^Awaiter: (member GetResult: unit -> ^T)

            /// Provides evidence that types following the task-like pattern can be used in 'return!' in a task computation expression
            [<NoDynamicInvocation>]
            static member inline CanReturnFrom< ^TaskLike, ^Awaitable, ^Awaiter, ^T
                                                    when ^TaskLike : (member ConfigureAwait : bool -> ^Awaitable)
                                                    and ^Awaitable : (member GetAwaiter : unit -> ^Awaiter)
                                                    and ^Awaiter :> ICriticalNotifyCompletion 
                                                    and ^Awaiter : (member get_IsCompleted : unit -> bool)
                                                    and ^Awaiter : (member GetResult : unit -> ^T) > : IPriority1 * configurableTaskLike: ^TaskLike -> TaskStep< ^T, ^T>

            /// Provides evidence that F# async computations can be used in 'return!' in a task computation expression
            [<NoDynamicInvocation>]
            static member inline CanReturnFrom: IPriority1 * computation: Async<'T> -> TaskStep<'T, 'T>

        type TaskBuilder with

            /// Provides the ability to bind to a variety of tasks, using context-sensitive semantics
            [<NoDynamicInvocation>]
            member inline Bind : task: ^TaskLike * continuation: (^TResult1 -> TaskCode<'TOverall, 'TResult2>) -> TaskCode<'TOverall, 'TResult2>
                when (TaskWitnesses or  ^TaskLike): (static member CanBind: TaskWitnesses * ^TaskLike * (^TResult1 -> TaskCode<'TOverall, 'TResult2>) -> TaskCode<'TOverall, 'TResult2>)

            /// Provides the ability to return results from a variety of tasks, using context-sensitive semantics
            [<NoDynamicInvocation>]
            member inline ReturnFrom: a: ^TaskLike -> TaskStep< 'T, 'T >
                when (TaskWitnesses or  ^TaskLike): (static member CanReturnFrom: TaskWitnesses * ^TaskLike -> TaskStep<'T, 'T>)
*)
#endif
