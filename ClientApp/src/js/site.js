import "bootstrap";
import "bootstrap/dist/css/bootstrap.css";

import "../css/site.css";
import "../scss/bootstrap-custom.scss";

// JQuery (need for dotnetcore)
// import * as $ from "jquery";
// import "jquery-validation";
// import "jquery-validation-unobtrusive";

// // Validation
// $.validator.addMethod("enforcetrue", function (value, element, param) {
//   return element.checked;
// });

// $.validator.unobtrusive.adapters.addBool("enforcetrue");

// $.validator.methods.range = function (value, element, param) {
//   let globalizedValue = value.replace(",", ".");
//   return this.optional(element) || (globalizedValue >= param[0] && globalizedValue <= param[1]);
// };

// $.validator.methods.number = function (value, element) {
//   return this.optional(element) || /-?(?:\d+|\d{1,3}(?:[\s\.,]\d{3})+)(?:[\.,]\d+)?$/.test(value);
// };
// //Date dd/MM/yyyy
// $.validator.methods.date = function (value, element) {
//   let date = value.split("/");
//   return this.optional(element) || !/Invalid|NaN/.test(new Date(date[2], date[1], date[0]).toString());
// };
