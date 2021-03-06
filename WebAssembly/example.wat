;; Simple WebAssembly example.

(module
  (import "math" "sin" (func $sin (param f32) (result f32)))
  (global $timesCalled (import "globalVars" "timesCalled") (mut i32))

  (func
    ;; Function signature
    (export "duplicate")
    (param $x i32)
    (result i32)

    ;; Increment the timesCalled global variable
    ;; timesCalled++
    global.get $timesCalled
    i32.const 1
    i32.add
    global.set $timesCalled

    ;; Body of the function
    local.get $x
    i32.const 2
    i32.mul
  )

  (func
    (export "foo")
    (param $a f32)
    (param $b f32)
    (result f32)

    ;; $a * 3 + $sin($b)
    local.get $a
    f32.const 3
    f32.mul
    local.get $b
    call $sin
    f32.add
  )
)
