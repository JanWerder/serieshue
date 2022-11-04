import "bootstrap";
import "bootstrap/dist/css/bootstrap.css";

import "../css/site.css";
import "../scss/bootstrap-custom.scss";

import Alpine from "alpinejs";
import persist from "@alpinejs/persist";

Alpine.plugin(persist);

window.Alpine = Alpine;

Alpine.start();