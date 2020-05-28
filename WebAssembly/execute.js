#!/usr/bin/env node

const fileName = './example.wat';
const fs = require('fs');
const wabt = require('wabt')();

const importArgs = {
  math: {
    sin: x => Math.sin(x)
  },
  globalVars: {
    timesCalled: new WebAssembly.Global({value: 'i32', mutable: true}, 0)
  }
};

async function getInstance() {
  try {
    const fileContent = fs.readFileSync(fileName, 'utf8');
    const wasmModule = wabt.parseWat(fileName, fileContent);
    const buffer = wasmModule.toBinary({}).buffer;
    const module = await WebAssembly.compile(buffer);
    return WebAssembly.instantiate(module, importArgs);
  } catch (error) {
    console.log(error.message);
    process.exit(1);
  }
}

async function main() {
  const instance = await getInstance();

  const duplicate = instance.exports.duplicate;
  console.log(importArgs.globalVars.timesCalled.value); // 0
  console.log(duplicate(5));
  console.log(importArgs.globalVars.timesCalled.value); // 1
  console.log(duplicate(15));
  console.log(importArgs.globalVars.timesCalled.value); // 2

  const foo = instance.exports.foo;
  console.log(foo(10.1, 5.2));
}

main();
