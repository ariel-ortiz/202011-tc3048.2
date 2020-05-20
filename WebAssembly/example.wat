;; Simple WebAssembly example.

(module
  (func
    ;; Function signature
    (export "duplicate")
    (param $x i32)
    (result i32)

    ;; Body of the function
    local.get $x
    i32.const 2
    i32.mul
  )
)
