// ** React Imports
import {useState, useEffect, MouseEvent, useCallback, ReactElement} from 'react'

// ** Next Import
import Link from 'next/link'

// ** MUI Imports
import Box from '@mui/material/Box'
import Card from '@mui/material/Card'
import Menu from '@mui/material/Menu'
import Grid from '@mui/material/Grid'
import {DataGrid} from '@mui/x-data-grid'
import MenuItem from '@mui/material/MenuItem'
import {styled} from '@mui/material/styles'
import IconButton from '@mui/material/IconButton'
import Typography from '@mui/material/Typography'
import CardHeader from '@mui/material/CardHeader'
import InputLabel from '@mui/material/InputLabel'
import FormControl from '@mui/material/FormControl'
import CardContent from '@mui/material/CardContent'
import Select, {SelectChangeEvent} from '@mui/material/Select'

// ** Icons Imports
import Laptop from 'mdi-material-ui/Laptop'
import ChartDonut from 'mdi-material-ui/ChartDonut'
import CogOutline from 'mdi-material-ui/CogOutline'
import EyeOutline from 'mdi-material-ui/EyeOutline'
import DotsVertical from 'mdi-material-ui/DotsVertical'
import PencilOutline from 'mdi-material-ui/PencilOutline'
import DeleteOutline from 'mdi-material-ui/DeleteOutline'
import AccountOutline from 'mdi-material-ui/AccountOutline'

// ** Store Imports
import {useDispatch, useSelector} from 'react-redux'

// ** Custom Components Imports
import CustomChip from 'src/@core/components/mui/chip'
import CustomAvatar from 'src/@core/components/mui/avatar'

// ** Utils Import
import {getInitials} from 'src/@core/utils/get-initials'

// ** Actions Imports
import {fetchData, deleteUser} from 'src/lib/store/apps/users/apis'

// ** Types Imports
import {RootState, AppDispatch} from 'src/lib/store'
import {ThemeColor} from 'src/@core/layouts/types'

// ** Custom Components Imports
import TableHeader from 'src/views/apps/user/list/TableHeader'
import AddUserDrawer from 'src/views/apps/user/list/AddUserDrawer'
import {UserType} from "src/types/apps/userTypes";

interface UserRoleType {
  [key: string]: ReactElement
}

interface UserStatusType {
  [key: string]: ThemeColor
}

// ** Vars
const userRoleObj: UserRoleType = {
  admin: <Laptop sx={{mr: 2, color: 'error.main'}}/>,
  supervisor: <CogOutline sx={{mr: 2, color: 'warning.main'}}/>,
  user: <PencilOutline sx={{mr: 2, color: 'info.main'}}/>,
  support: <ChartDonut sx={{mr: 2, color: 'success.main'}}/>,
}

interface CellType {
  row: UserType
}

const userStatusObj: UserStatusType = {
  active: 'success',
  pending: 'warning',
  inactive: 'secondary'
}

// ** Styled component for the link for the avatar with image
const AvatarWithImageLink = styled(Link)(({theme}) => ({
  marginRight: theme.spacing(3)
}))

// ** Styled component for the link for the avatar without image
const AvatarWithoutImageLink = styled(Link)(({theme}) => ({
  textDecoration: 'none',
  marginRight: theme.spacing(3)
}))

// ** renders client column
const renderClient = (row: UserType) => {
  if (row.imagePath?.length) {
    return (
      <AvatarWithImageLink href={`/user/view/${row.id}`}>
        <CustomAvatar src={row.imagePath} sx={{mr: 3, width: 34, height: 34}}/>
      </AvatarWithImageLink>
    )
  } else {
    return (
      <AvatarWithoutImageLink href={`/user/view/${row.id}`}>
        <CustomAvatar
          skin='light'
          color={row.avatarColor || 'primary'}
          sx={{mr: 3, width: 34, height: 34, fontSize: '1rem'}}
        >
          {getInitials(row.fullName ?? '______')}
        </CustomAvatar>
      </AvatarWithoutImageLink>
    )
  }
}

// ** Styled component for the link inside menu
const MenuItemLink = styled('a')(({theme}) => ({
  width: '100%',
  display: 'flex',
  alignItems: 'center',
  textDecoration: 'none',
  padding: theme.spacing(1.5, 4),
  color: theme.palette.text.primary
}))

const RowOptions = ({id}: { id: number | string }) => {
  // ** Hooks
  const dispatch = useDispatch<AppDispatch>()

  // ** State
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null)

  const rowOptionsOpen = Boolean(anchorEl)

  const handleRowOptionsClick = (event: MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget)
  }
  const handleRowOptionsClose = () => {
    setAnchorEl(null)
  }

  const handleDelete = () => {
    dispatch(deleteUser(id))
    handleRowOptionsClose()
  }

  return (
    <>
      <IconButton size='small' onClick={handleRowOptionsClick}>
        <DotsVertical/>
      </IconButton>
      <Menu
        keepMounted
        anchorEl={anchorEl}
        open={rowOptionsOpen}
        onClose={handleRowOptionsClose}
        anchorOrigin={{
          vertical: 'bottom',
          horizontal: 'right'
        }}
        transformOrigin={{
          vertical: 'top',
          horizontal: 'right'
        }}
        PaperProps={{style: {minWidth: '8rem'}}}
      >
        <MenuItem sx={{p: 0}}>
          <Link href={`/user/view/${id}`} passHref>
            <MenuItemLink>
              <EyeOutline fontSize='small' sx={{mr: 2}}/>
              View
            </MenuItemLink>
          </Link>
        </MenuItem>
        <MenuItem onClick={handleRowOptionsClose}>
          <PencilOutline fontSize='small' sx={{mr: 2}}/>
          Edit
        </MenuItem>
        <MenuItem onClick={handleDelete}>
          <DeleteOutline fontSize='small' sx={{mr: 2}}/>
          Delete
        </MenuItem>
      </Menu>
    </>
  )
}

const columns = [
  {
    flex: 0.2,
    minWidth: 230,
    field: 'fullName',
    headerName: 'User',
    renderCell: ({row}: CellType) => {
      const {id, fullName, userName} = row

      return (
        <Box sx={{display: 'flex', alignItems: 'center'}}>
          {renderClient(row)}
          <Box sx={{display: 'flex', alignItems: 'flex-start', flexDirection: 'column'}}>
            <Link href={`/user/view/${id}`} passHref>
              <Typography
                noWrap
                component='a'
                variant='subtitle2'
                sx={{color: 'text.primary', textDecoration: 'none'}}
              >
                {fullName}
              </Typography>
            </Link>
            <Link href={`/user/view/${id}`} passHref>
              <Typography noWrap component='a' variant='caption' sx={{textDecoration: 'none'}}>
                @{userName}
              </Typography>
            </Link>
          </Box>
        </Box>
      )
    }
  },
  {
    flex: 0.2,
    minWidth: 250,
    field: 'email',
    headerName: 'Email',
    renderCell: ({row}: CellType) => {
      return (
        <Typography noWrap variant='body2'>
          {row.email}
        </Typography>
      )
    }
  },
  {
    flex: 0.15,
    field: 'role',
    minWidth: 150,
    headerName: 'Role',
    renderCell: ({row}: CellType) =>
      row.roles.map(role => (
        <Box key={role} sx={{display: 'flex', alignItems: 'center'}}>
          {userRoleObj[role]}
          <Typography noWrap sx={{color: 'text.secondary', textTransform: 'capitalize'}}>
            {role}
          </Typography>
        </Box>
      ))
  },
  {
    flex: 0.1,
    minWidth: 110,
    field: 'status',
    headerName: 'Status',
    renderCell: ({row}: CellType) => {
      return (
        <CustomChip
          skin='light'
          size='small'
          label={row.status}
          color={userStatusObj[row.status]}
          sx={{textTransform: 'capitalize', '& .MuiChip-label': {lineHeight: '18px'}}}
        />
      )
    }
  },
  {
    flex: 0.1,
    minWidth: 90,
    sortable: false,
    field: 'actions',
    headerName: 'Actions',
    renderCell: ({row}: CellType) => <RowOptions id={row.id}/>
  }
]

const UserList = () => {
  // ** State
  const [role, setRole] = useState<string>('')
  const [value, setValue] = useState<string>('')
  const [status, setStatus] = useState<string>('')
  const [pageSize, setPageSize] = useState<number>(10)
  const [addUserOpen, setAddUserOpen] = useState<boolean>(false)

  // ** Hooks
  const dispatch = useDispatch<AppDispatch>()
  const store = useSelector((state: RootState) => state.users)

  useEffect(() => {
    dispatch(
      fetchData({
        role,
        status,
        q: value,
        pageNumber: 1,
        pageSize: 10
      })
    )
  }, [dispatch, role, status, value])

  const handleFilter = useCallback((val: string) => {
    setValue(val)
  }, [])

  const handleRoleChange = useCallback((e: SelectChangeEvent) => {
    setRole(e.target.value)
  }, [])

  const handleStatusChange = useCallback((e: SelectChangeEvent) => {
    setStatus(e.target.value)
  }, [])

  const toggleAddUserDrawer = () => setAddUserOpen(!addUserOpen)

  return (
    <Grid container spacing={6}>
      <Grid item xs={12}>
        <Card>
          <CardHeader title='Search Filters' sx={{pb: 4, '& .MuiCardHeader-title': {letterSpacing: '.15px'}}}/>
          <CardContent>
            <Grid container spacing={6}>
              <Grid item sm={4} xs={12}>
                <FormControl fullWidth>
                  <InputLabel id='role-select'>Select Role</InputLabel>
                  <Select
                    fullWidth
                    value={role}
                    id='select-role'
                    label='Select Role'
                    labelId='role-select'
                    onChange={handleRoleChange}
                    inputProps={{placeholder: 'Select Role'}}
                  >
                    <MenuItem value=''>Select Role</MenuItem>
                    <MenuItem value='admin'>Admin</MenuItem>
                    <MenuItem value='author'>Author</MenuItem>
                    <MenuItem value='editor'>Editor</MenuItem>
                    <MenuItem value='maintainer'>Maintainer</MenuItem>
                    <MenuItem value='subscriber'>Subscriber</MenuItem>
                  </Select>
                </FormControl>
              </Grid>
              <Grid item sm={4} xs={12}>
                <FormControl fullWidth>
                  <InputLabel id='status-select'>Select Status</InputLabel>
                  <Select
                    fullWidth
                    value={status}
                    id='select-status'
                    label='Select Status'
                    labelId='status-select'
                    onChange={handleStatusChange}
                    inputProps={{placeholder: 'Select Role'}}
                  >
                    <MenuItem value=''>Select Role</MenuItem>
                    <MenuItem value='pending'>Pending</MenuItem>
                    <MenuItem value='active'>Active</MenuItem>
                    <MenuItem value='inactive'>Inactive</MenuItem>
                  </Select>
                </FormControl>
              </Grid>
            </Grid>
          </CardContent>
        </Card>
      </Grid>
      <Grid item xs={12}>
        <Card>
          <TableHeader value={value} handleFilter={handleFilter} toggle={toggleAddUserDrawer}/>
          <DataGrid
            autoHeight
            rows={store.data}
            columns={columns}
            checkboxSelection
            pageSize={pageSize}
            disableSelectionOnClick
            rowsPerPageOptions={[10, 25, 50]}
            sx={{'& .MuiDataGrid-columnHeaders': {borderRadius: 0}}}
            onPageSizeChange={(newPageSize: number) => setPageSize(newPageSize)}
          />
        </Card>
      </Grid>

      <AddUserDrawer open={addUserOpen} toggle={toggleAddUserDrawer}/>
    </Grid>
  )
}

export default UserList
