

async function fetchValues() {
    return fetch('https://localhost:5001/api/values', {
        mode: 'cors'
    } as RequestInit)
}

export { fetchValues };