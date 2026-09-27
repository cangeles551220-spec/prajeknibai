document.querySelectorAll('[data-password-target]').forEach(button => {
  button.addEventListener('click', () => {
    const input = document.getElementById(button.dataset.passwordTarget);
    if (!input) return;
    const showing = input.type === 'text';
    input.type = showing ? 'password' : 'text';
    button.setAttribute('aria-label', showing ? 'Show password' : 'Hide password');
    button.setAttribute('title', showing ? 'Show password' : 'Hide password');
    button.textContent = showing ? '◉' : '◎';
  });
});

const loginScreen = document.querySelector('.login-screen');
const loginForm = document.querySelector('form[action="/Auth/Login"], form[action*="Auth/Login"]') || document.querySelector('form');
const signInButton = document.querySelector('.login-button');

if (loginScreen) {
  loginScreen.classList.remove('transitioning');
}

if (loginForm && signInButton) {
  loginForm.addEventListener('submit', (event) => {
    if (!loginForm.checkValidity()) {
      return;
    }

    event.preventDefault();
    signInButton.classList.add('is-loading');
    signInButton.disabled = true;
    signInButton.textContent = 'Signing in...';

    if (loginScreen) {
      loginScreen.classList.add('transitioning');
    }

    requestAnimationFrame(() => {
      setTimeout(() => {
        loginForm.submit();
      }, 420);
    });
  });
}

const appShell = document.getElementById('appShell');
if (appShell) {
  requestAnimationFrame(() => {
    appShell.classList.add('is-visible');
  });
}
