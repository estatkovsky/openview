const webpack = require("webpack");
const HtmlWebpackPlugin = require('html-webpack-plugin')
const merge = require('webpack-merge');
const common = require('./webpack.common.js');

module.exports = merge(common, {
  mode: 'development',
  output: {
    filename: 'main.js'
  },
  devtool: 'inline-source-map',
  devServer: {
    inline: true,
    hot: true,
    overlay: true,
  },
  plugins: [
    new webpack.HotModuleReplacementPlugin(),
    new webpack.NoEmitOnErrorsPlugin(),
    new HtmlWebpackPlugin()
  ]
});