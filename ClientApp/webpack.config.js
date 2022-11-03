const path = require("path");
const MiniCssExtractPlugin = require("mini-css-extract-plugin");

module.exports = {
  entry: {
    site: "./src/js/site.js",
  },
  output: {
    filename: "[name].entry.js",
    path: path.resolve(__dirname, "..", "wwwroot", "dist"),
  },
  devtool: "source-map",
  mode: "development",
  module: {
    rules: [
      {
        test: /\.(sa|sc|c)ss$/,
        use: [MiniCssExtractPlugin.loader, "css-loader", "postcss-loader", "sass-loader"],
      },
      {
        test: /\.(eot|woff(2)?|ttf|otf|svg)$/i,
        type: "asset",
      },
    ],
  },
  plugins: [new MiniCssExtractPlugin({ filename: "style.css", chunkFilename: "[name].css" })],
};
