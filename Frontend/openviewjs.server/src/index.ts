import 'idempotent-babel-polyfill';
import 'isomorphic-fetch';
import * as React from 'react';
import * as ReactDOM from 'react-dom';
import { AppComponent } from './components/app';

import './scss/main.scss';
import 'font-awesome/css/font-awesome.css';

function preRenderSetup() {
    window.onerror = (msg, url, line, column, stack) => {
        let message = 'msg: ' + msg + ' row: ' + line + ' col: ' + column + ' stack: ' + stack + ' url: ' + url;
        console.error(message);
    };
}

export function renderRootComponent() {
    preRenderSetup();
    let root = document.getElementById('root');
    if (!root) {
        root = document.createElement('div');
        root.id = 'root';
        document.body.appendChild(root)
    }
    ReactDOM.render(
        React.createElement(AppComponent),
        root);
}
renderRootComponent();

declare var module: any;

if (module.hot) {
    module.hot.accept('./components/app', renderRootComponent);
}
