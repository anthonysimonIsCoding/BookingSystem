import { Navigate } from "react-router-dom";
import { useAuth } from "../hook/useAuth";

function ProtectedRoute({ children }: any) {
    const { isCustomerLoggedIn } = useAuth();   // dùng context thay vì localStorage trực tiếp

    if (!isCustomerLoggedIn) {
        return <Navigate to="/login" replace />;
    }

    return children;
}

export default ProtectedRoute;