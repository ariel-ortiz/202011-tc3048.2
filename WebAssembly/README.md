# Simple Example using WebAssembly

You need at least version 12 of Node.js. If you need to install it go to
https://nodejs.org/en/download/ or, if you're using Ubuntu, type at the
terminal:

    curl -sL https://deb.nodesource.com/setup_12.x | sudo bash -

    sudo apt install nodejs

To check that you have the correct version of Node.js, at the terminal type:

    node -v

The output should show that you are using version 12 or higher:

    v12.16.3

You can now run the following commands at the terminal:

    npm init -y

    npm install wabt --save

    node execute.js
