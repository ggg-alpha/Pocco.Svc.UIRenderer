const mdIt = window.markdownit();

function scrollToBottom() {
  try {
    const messageHolderEl = document.getElementById('message_holder');
    messageHolderEl.scrollTop = messageHolderEl.scrollHeight;
  } catch (error) {
    console.error(error);
  }
}

function markdownStringToHtml(content, id) {
  const el = document.getElementById(id);
  el.classList.add("markdown-content");
  el.innerHTML = mdIt.render(content);
}

function createMessage(id, username, content, createdAt, useCdn = "http://localhost:5197") {
  const parentEl = document.getElementById('message_holder');

  const messageEl = document.createElement('div');
  messageEl.id = id;
  messageEl.classList.add("w-100", "d-flex", "flex-row", "justify-content-start", "align-items-start", "gap-1")
  messageEl.innerHTML = `
  <img src="http://${useCdn}/sys-content/images/120x120.png" alt="User icon" style="width: 45px; height: auto; border-radius: 50%;" />
  <div>
    <div class="d-flex flex-row gap-2 justify-content-start align-items-end" style="color: rgb(228, 228, 228);">
      <h4>${username}</h4>
      <p>${createdAt}</p>
    </div>
    <div id="${id}-content"></div>
  </div>
  `;
  parentEl.appendChild(messageEl);

  const messageContentEl = document.getElementById(`${id}-content`);
  window.MessageContentHelper.markdownStringToHtml(content, messageContentEl.id);

  window.MessageContentHelper.scrollToBottom();
}

function clearMessages() {
  const parentEl = document.getElementById('message_holder');
  parentEl.innerHTML = '';
}

window.MessageContentHelper = {
  scrollToBottom,
  markdownStringToHtml,
  createMessage,
  clearMessages
}
