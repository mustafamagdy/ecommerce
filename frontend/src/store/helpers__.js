/*
// *******************************************************************
// Getters
// *******************************************************************
// Find an object in a list of objects by matching a property value.
// userById: findByKey('users', 'id')
// userById: findByKey(['org', 'users'], 'id')
// getters.userById('123')
export function findByKey(keyPath, targetKey) {
    return state => {
        let key = keyPath;
        if (Array.isArray(keyPath)) {
            state = traversePath(state, keyPath, false);
            key = keyPath[keyPath.length - 1];
        }
        const find = val => state[key].find(x => x[targetKey] === val);
        return val => Array.isArray(val) ? val.map(find) : find(val);
    };
}

// Filter a list of objects by matching a property value.
// usersByStatus: filterByKey('users', 'status')
// usersByStatus: filterByKey(['org', 'users'], 'status')
// getters.usersByStatus('INACTIVE')
// getters.usersByStatus(['ACTIVE', 'INACTIVE'])
export function filterByKey(keyPath, targetKey) {
    return state => {
        let key = keyPath;
        if (Array.isArray(keyPath)) {
            state = traversePath(state, keyPath, false);
            key = keyPath[keyPath.length - 1];
        }
        return vals => {
            if (!Array.isArray(vals)) vals = [vals];
            return state[key].filter(x => vals.includes(x[targetKey]));
        };
    };
}

// *******************************************************************
// Mutators
// *******************************************************************
// increment the index of a list argument or a list in state
export const adjustListIndex = (key, targetKeyOrList) => (state, n = 1) => {
    const list = Array.isArray(targetKeyOrList) ? targetKeyOrList : state[targetKeyOrList];
    const idx = (state[key] + Math.floor(n)) % list.length;
    state[key] = idx >= 0 ? idx : list.length + idx;
};

// copy all key/values from data to state
// useful for resetting state to default values
// resetState: assignConstant(initialState)
// commit('resetState')
export const assignConstant = data => state => {
    Object.assign(state, data);
};

// add or extend a record in a list
export const extendRecordInList = (key, targetKey = "id", valKey) => (state, data) => {
    const id = data[targetKey];
    const val = valKey ? data[valKey] : data;
    const index = state[key].findIndex(x => x[targetKey] === id);
    return index < 0
        ? state[key].push(val)
        : state[key].splice(index, 1, Object.assign({}, state[key][index], val));
};

// push an item onto a list
// addItem: pushTo('items')
export const pushTo = key => (state, val) => {
    state[key].push(val);
};
// concat two arrays
// addItem: mergeWith('items')
export const mergeWith = key => (state, val) => {
    state[key].concat(val);
};
// add or replace a record in a list
export const replaceRecordInList = (key, targetKey = "id", valKey) => (state, data) => {
    const id = data[targetKey];
    const val = valKey ? data[valKey] : data;
    const index = state[key].findIndex(x => x[targetKey] === id);
    return index < 0
        ? state[key].push(val)
        : state[key].splice(index, 1, val);
};

// Abstraction over setProp and setPath
export const set = key => Array.isArray(key) ? setPath(key) : setProp(key);

// Set a value at a path within state
// Creates objects and arrays as needed
// Path is an array, and array indicies are numbers (not string numbers)
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

// Toggle boolean in state
// toggleOpen: toggle('open')
// commit('toggleOpen')
export const toggle = key => state => {
    state[key] = !state[key];
};

// remove item or object from list
export const without = (key, targetKey) => (state, items) => {
    if (!Array.isArray(items)) items = [items];
    state[key] = state[key].filter(x => targetKey
        ? !items.some(y => y === x[targetKey])
        : !items.includes(x)
    );
};

export const addRecord = () => (state, record) => {
    if (Array.isArray(record)) {
        for (let index = 0; index < record.length; index++) {
            state.records.unshift(record[index]);
        }
    } else {
        state.records.unshift(record);
        state.currentRecord = record;
    }
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
*/
