const mdIt = window.markdownit();
function markdownStringToHtml(content, id) {
  const el = document.getElementById(id);
  el.innerHTML = mdIt.render(content);
}

window.MessageContentHelper = {
  markdownStringToHtml
}
