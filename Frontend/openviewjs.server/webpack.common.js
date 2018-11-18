const path = require('path');

var basePath = __dirname;

module.exports = {
  context: path.join(basePath, 'src'),
  resolve: {
    extensions: ['.js', '.ts', '.tsx']
  },
  entry: ['idempotent-babel-polyfill', './index.ts'],
  output: {
    path: path.join(basePath, 'dist'),
    filename: '[name].js'
  },
  module: {
    rules: [{
        test: /\.tsx?$/,
        exclude: /node_modules/,
        loaders: ['babel-loader', 'ts-loader']
      },
      {
        test: /\.(scss)$/,
        use: [{
          loader: 'style-loader',
        }, {
          loader: 'css-loader',
        }, {
          loader: 'postcss-loader',
          options: {
            plugins: function () {
              return [
                require('precss'),
                require('autoprefixer')
              ];
            }
          }
        }, {
          loader: 'sass-loader'
        }]
      },
      {
        test: /\.(css)$/,
        use: [{
          loader: 'style-loader',
        }, {
          loader: 'css-loader',
        }]
      },
      {
        test: /\.(png|eot|tiff|svg|woff2|woff|ttf|gif|mp3|jpg)$/,
        use: [{
          loader: 'file-loader',
          options: {
            name: 'files/[hash].[ext]',
          },
        }],
      },
    ]
  }
};