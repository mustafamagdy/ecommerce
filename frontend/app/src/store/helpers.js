// *******************************************************************
// Mutators
// *******************************************************************

// Abstraction over setProp and setPath
export const set = key => Array.isArray(key) ? setPath(key) : setProp(key);

// Set a value at a path within state
// Creates objects and arrays as needed
// Path is an array, and array indices are numbers (not string numbers)
// setUsername: setPath(['user', 'username'])
// commit('setUsername', 'foo')
export const setPath = path => (state, val) => {
    const obj = traversePath(state, path, true);
    obj[path[path.length - 1]] = val;
};

// Set property on state
// setUser: set('user')
// commit('setUser', { name: 'foo' })
export const setProp = key => (state, val) => {
    state[key] = val;
};

// concat two arrays
// addItem: mergeWith('items')
export const mergeWith = key => (state, val) => {
    state[key].concat(val);
};

// *******************************************************************
// Helpers
// *******************************************************************

export function traversePath(state, path, createPathObjects) {
    return path.slice(0, -1).reduce((acc, x, i) => {
        if (!(x in acc)) {
            if (createPathObjects) {
                acc[x] = typeof path[i + 1] === "number" ? [] : {};
            } else {
                throw new Error(`Vuex intern: path ${JSON.stringify(path.slice(0, -1))} not in state`);
            }
        }
        return acc[x];
    }, state);
}
