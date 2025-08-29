document.getElementById('Sign-Out')?.addEventListener('click', async (e) => {
    e.preventDefault();
    try {
        await axios.post('/api/auth/logout', {}, { withCredentials: true });
        location.href = '/account/login';
    } catch (error) {
        location.href = '/account/login';
    }
});
