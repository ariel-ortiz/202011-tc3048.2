(module
  (import "math" "pow" (func $pow (param i32) (param i32) (result i32)))
  (func
    (export "start")
    (result i32)
    i32.const 2
    i32.const 3
    i32.const 2
    call $pow
    call $pow
  )
)