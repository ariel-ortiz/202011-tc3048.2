#!/usr/bin/env node

const fileName = './example.wat';
const fs = require('fs');
const wabt = require('wabt')();

const importArgs = {
  math: {
    sin: x => Math.sin(x)
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
  console.log(duplicate(5));

  const foo = instance.exports.foo;
  console.log(foo(10.1, 5.2));
}

main();
