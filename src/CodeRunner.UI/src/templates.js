export default {
    "csharp" : [
        {
            processCount: 1,
            code: `
using System;

Console.WriteLine("Hello World");`
    },
    {
        processCount: 5,
        code: `
using System;
using System.Diagnostics;

Console.WriteLine("Hi from process #" + Process.GetCurrentProcess().Id);`
    },
    {
        processCount: 3,
        code: `
using System;

for(var i = 0; i < 10; i++)
    Console.WriteLine(i);`
    }
],

    "golang": [
    {
        processCount: 2,
        code: `
package main

import (
    "fmt"
)

func main() {
    fmt.Printf("Hello World")
}`
    }, 
    {
        processCount: 1,
        code: `
package main

import (
    "fmt"
    "sync"
    "time"
)

func main() {

    wg := &sync.WaitGroup{}

    for i := 0; i < 10; i++ {
        go func(a int) {
            wg.Add(1)
            defer wg.Done()

            fmt.Println(a)
        }(i)
    }

    time.Sleep(1*time.Second)
    wg.Wait()
}`
    },
    {
        processCount: 1,
        code: `
package main

import (
    "fmt"
    "sync"
)

func main() {

    wg := &sync.WaitGroup{}
    ch := make(chan int)

    wg.Add(2)
    go func() {
        defer wg.Done()
        producer(ch)
    }()

    go func() {
        defer wg.Done()
        consumer(ch)
    }()

    wg.Wait()
}

func producer(ch chan<- int) {
    defer close(ch)

    for i := 0; i < 10; i++ {
        ch <- i
        fmt.Printf("Produce - %v\\n", i)
    }
}

func consumer(ch <-chan int) {
    for v := range ch {
        fmt.Printf("Consume - %v\\n", v)
    }
}`
    }
]
}