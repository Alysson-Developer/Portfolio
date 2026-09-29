document.addEventListener("keydown", event => {
  if (event.key !== "Enter" || !event.target.matches(".admin-tag-add input")) return;
  event.preventDefault();
  event.target.parentElement.querySelector("button")?.click();
});
